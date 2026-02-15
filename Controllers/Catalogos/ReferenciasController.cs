using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels.Catalogos.Referencias;

namespace InventarioWEB.Controllers.Catalogos
{
    /// <summary>
    /// Controlador responsable de la administración del catálogo de Referencias.
    /// 
    /// Funcionalidades implementadas:
    /// - Listado de referencias activas.
    /// - Creación de nuevas referencias asociadas a un género.
    /// - Edición de referencias existentes.
    /// - Eliminación lógica (soft delete).
    /// - Recuperación de registros inactivos.
    /// 
    /// Buenas prácticas aplicadas:
    /// - Uso de ViewModels para desacoplar entidades.
    /// - Validaciones del lado del servidor.
    /// - Control de duplicidad por género.
    /// - Ordenamiento estructurado por clave foránea.
    /// - Protección contra eliminación con dependencias activas.
    /// </summary>
    public class ReferenciasController : Controller
    {
        /// <summary>
        /// Contexto de base de datos inyectado mediante
        /// el mecanismo de Dependency Injection.
        /// </summary>
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Constructor del controlador.
        /// Inicializa el acceso al contexto de datos.
        /// </summary>
        public ReferenciasController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ====================================================
        // INDEX (SOLO ACTIVOS)
        // ====================================================
        /// <summary>
        /// Muestra el listado de referencias activas.
        /// 
        /// Características:
        /// - Filtra únicamente registros con Activo = true.
        /// - Ordena primero por ID_Genero y luego por descripción.
        /// - Proyecta hacia ViewModel para evitar exponer entidades.
        /// - Optimiza consulta usando AsNoTracking().
        /// </summary>
        /// <returns>
        /// Vista con las referencias activas organizadas por género.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            var referencias = await _context.Referencias
                .AsNoTracking()
                .Where(r => r.Activo)
                .OrderBy(r => r.ID_Genero)
                .ThenBy(r => r.DescripReferencia)
                .Select(r => new ReferenciaItemVM
                {
                    ID_Referencias = r.ID_Referencias,
                    DescripReferencia = r.DescripReferencia,
                    Genero = r.Genero.DescripGenero
                })
                .ToListAsync();

            return View(new ReferenciasIndexVM
            {
                Referencias = referencias
            });
        }

        // ====================================================
        // CREAR
        // ====================================================
        /// <summary>
        /// Renderiza la vista de creación de una nueva referencia.
        /// Carga el listado de géneros disponibles para selección.
        /// </summary>
        public async Task<IActionResult> Crear()
        {
            return View(new ReferenciaCreateVM
            {
                Generos = await ObtenerGenerosSelectList()
            });
        }

        /// <summary>
        /// Procesa la creación de una nueva referencia.
        /// 
        /// Validaciones implementadas:
        /// - Modelo válido.
        /// - Control de duplicidad por género.
        /// - Normalización de texto para comparación insensible a mayúsculas.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ReferenciaCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Generos = await ObtenerGenerosSelectList();
                return View(model);
            }

            var descripcion = model.DescripReferencia.Trim();
            var descripcionNormalizada = descripcion.ToUpper();

            // Verifica que no exista referencia activa con misma descripción y género
            bool existe = await _context.Referencias.AnyAsync(r =>
                r.Activo &&
                r.ID_Genero == model.ID_Genero &&
                r.DescripReferencia.ToUpper() == descripcionNormalizada);

            if (existe)
            {
                ModelState.AddModelError(nameof(model.DescripReferencia),
                    "Ya existe una referencia con esa descripción para el género seleccionado.");
                model.Generos = await ObtenerGenerosSelectList();
                return View(model);
            }

            // Creación del registro
            var referencia = new Referencia
            {
                DescripReferencia = descripcion,
                ID_Genero = model.ID_Genero,
                Activo = true
            };

            _context.Referencias.Add(referencia);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Referencia creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // EDITAR
        // ====================================================
        /// <summary>
        /// Carga el formulario de edición de una referencia activa.
        /// </summary>
        /// <param name="id">Identificador de la referencia.</param>
        public async Task<IActionResult> Editar(int id)
        {
            var referencia = await _context.Referencias
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ID_Referencias == id && r.Activo);

            if (referencia == null)
                return NotFound();

            return View(new ReferenciaEditVM
            {
                ID_Referencias = referencia.ID_Referencias,
                DescripReferencia = referencia.DescripReferencia,
                ID_Genero = referencia.ID_Genero,
                Generos = await ObtenerGenerosSelectList()
            });
        }

        /// <summary>
        /// Procesa la actualización de una referencia existente.
        /// 
        /// Validaciones:
        /// - Coincidencia de identificador.
        /// - Modelo válido.
        /// - Registro activo existente.
        /// - Control de duplicidad por género.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ReferenciaEditVM model)
        {
            if (id != model.ID_Referencias)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                model.Generos = await ObtenerGenerosSelectList();
                return View(model);
            }

            var referencia = await _context.Referencias.FindAsync(id);

            if (referencia == null || !referencia.Activo)
                return NotFound();

            var descripcion = model.DescripReferencia.Trim();
            var descripcionNormalizada = descripcion.ToUpper();

            // Validación de duplicidad excluyendo el registro actual
            bool duplicado = await _context.Referencias.AnyAsync(r =>
                r.Activo &&
                r.ID_Referencias != id &&
                r.ID_Genero == model.ID_Genero &&
                r.DescripReferencia.ToUpper() == descripcionNormalizada);

            if (duplicado)
            {
                ModelState.AddModelError(nameof(model.DescripReferencia),
                    "Ya existe una referencia con esa descripción para el género seleccionado.");
                model.Generos = await ObtenerGenerosSelectList();
                return View(model);
            }

            referencia.DescripReferencia = descripcion;
            referencia.ID_Genero = model.ID_Genero;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Referencia actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // ELIMINAR (LÓGICO)
        // ====================================================
        /// <summary>
        /// Realiza eliminación lógica de la referencia.
        /// 
        /// Regla de negocio:
        /// - No permite eliminar si está asociada a productos.
        /// - Conserva trazabilidad histórica.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var referencia = await _context.Referencias.FindAsync(id);

            if (referencia == null)
                return NotFound();

            bool enUso = await _context.Productos
                .AnyAsync(p => p.ID_Referencias == id);

            if (enUso)
            {
                TempData["Error"] =
                    "No se puede eliminar la referencia porque está asociada a productos.";
                return RedirectToAction(nameof(Index));
            }

            referencia.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Referencia eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // RECUPERAR
        // ====================================================
        /// <summary>
        /// Reactiva una referencia previamente eliminada de forma lógica.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recuperar(int id)
        {
            var referencia = await _context.Referencias.FindAsync(id);

            if (referencia == null)
                return NotFound();

            referencia.Activo = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Referencia recuperada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // SELECT LIST GÉNEROS
        // ====================================================
        /// <summary>
        /// Obtiene el listado de géneros para ser utilizado
        /// en controles tipo Select (DropDownList).
        /// 
        /// Ordena los registros por identificador primario.
        /// </summary>
        private async Task<List<SelectListItem>> ObtenerGenerosSelectList()
        {
            return await _context.Generos
                .AsNoTracking()
                .OrderBy(g => g.ID_Genero)
                .Select(g => new SelectListItem
                {
                    Value = g.ID_Genero.ToString(),
                    Text = g.DescripGenero
                })
                .ToListAsync();
        }
    }
}
