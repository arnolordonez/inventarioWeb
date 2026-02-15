using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels.Catalogos.Colores;
using System.Linq;

namespace InventarioWEB.Controllers.Catalogos
{
    /// <summary>
    /// Controlador responsable de la gestión del catálogo de Colores.
    /// 
    /// Funcionalidades principales:
    /// - Listado de colores activos e inactivos.
    /// - Creación de nuevos colores.
    /// - Edición de colores existentes.
    /// - Eliminación lógica (soft delete).
    /// - Restauración de registros eliminados.
    /// 
    /// Aplica principios de:
    /// - Arquitectura MVC.
    /// - Validación de datos del lado del servidor.
    /// - Eliminación lógica mediante campo "Activo".
    /// - Control de integridad referencial con entidades relacionadas (Productos).
    /// </summary>
    public class ColoresController : Controller
    {
        /// <summary>
        /// Contexto de base de datos inyectado mediante
        /// inyección de dependencias (Dependency Injection).
        /// </summary>
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Constructor del controlador.
        /// Recibe el contexto de base de datos para permitir
        /// el acceso a las entidades del sistema.
        /// </summary>
        public ColoresController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ====================================================
        // INDEX (ACTIVOS + INACTIVOS)
        // ====================================================
        /// <summary>
        /// Muestra el listado completo del catálogo de colores,
        /// separando visualmente los registros activos e inactivos.
        /// 
        /// Características técnicas:
        /// - Uso de AsNoTracking() para optimizar consultas de solo lectura.
        /// - Ordenamiento alfabético insensible a mayúsculas/minúsculas.
        /// - Proyección hacia ViewModels para evitar exponer entidades.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Consulta de colores activos ordenados alfabéticamente
            var coloresActivos = await _context.Colores
                .AsNoTracking()
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre.ToUpper())
                .Select(c => new ColorListadoVM
                {
                    ID_Color = c.ID_Color,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            // Consulta de colores inactivos ordenados alfabéticamente
            var coloresInactivos = await _context.Colores
                .AsNoTracking()
                .Where(c => !c.Activo)
                .OrderBy(c => c.Nombre.ToUpper())
                .Select(c => new ColorListadoVM
                {
                    ID_Color = c.ID_Color,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            // Construcción del ViewModel principal
            var vm = new ColoresIndexVM
            {
                Colores = coloresActivos,
                ColoresInactivos = coloresInactivos
            };

            return View(vm);
        }

        // ====================================================
        // CREAR
        // ====================================================
        /// <summary>
        /// Renderiza la vista para creación de un nuevo color.
        /// </summary>
        public IActionResult Crear()
        {
            return View(new ColorCreateVM());
        }

        /// <summary>
        /// Procesa la creación de un nuevo color.
        /// 
        /// Validaciones implementadas:
        /// - Validación del modelo.
        /// - Eliminación de espacios innecesarios.
        /// - Verificación de nombre no vacío.
        /// - Validación de duplicado (activo o inactivo).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ColorCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nombre = model.Nombre?.Trim();

            // Validación de contenido válido
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError(nameof(model.Nombre),
                    "El nombre no es válido.");
                return View(model);
            }

            var nombreNormalizado = nombre.ToUpper();

            // Verificación de existencia previa (sin importar estado)
            bool existe = await _context.Colores
                .AnyAsync(c => c.Nombre.ToUpper() == nombreNormalizado);

            if (existe)
            {
                ModelState.AddModelError(nameof(model.Nombre),
                    "Ya existe un color con ese nombre (activo o inactivo).");
                return View(model);
            }

            // Creación del nuevo registro
            var color = new Color
            {
                Nombre = nombre,
                Activo = true
            };

            _context.Colores.Add(color);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Color creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // EDITAR
        // ====================================================
        /// <summary>
        /// Carga el formulario de edición de un color activo.
        /// </summary>
        public async Task<IActionResult> Editar(int id)
        {
            var color = await _context.Colores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID_Color == id && c.Activo);

            if (color == null)
                return NotFound();

            return View(new ColorEditVM
            {
                ID_Color = color.ID_Color,
                Nombre = color.Nombre
            });
        }

        /// <summary>
        /// Procesa la actualización de un color existente.
        /// 
        /// Validaciones:
        /// - Coincidencia de identificadores.
        /// - Modelo válido.
        /// - Existencia del registro activo.
        /// - Control de duplicidad.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ColorEditVM model)
        {
            if (id != model.ID_Color)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var color = await _context.Colores.FindAsync(id);

            if (color == null || !color.Activo)
                return NotFound();

            var nombre = model.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError(nameof(model.Nombre),
                    "El nombre no es válido.");
                return View(model);
            }

            var nombreNormalizado = nombre.ToUpper();

            // Validación de duplicados excluyendo el registro actual
            bool duplicado = await _context.Colores
                .AnyAsync(c =>
                    c.ID_Color != id &&
                    c.Nombre.ToUpper() == nombreNormalizado);

            if (duplicado)
            {
                ModelState.AddModelError(nameof(model.Nombre),
                    "Ya existe un color con ese nombre.");
                return View(model);
            }

            color.Nombre = nombre;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Color actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // ELIMINAR (SOFT DELETE)
        // ====================================================
        /// <summary>
        /// Realiza eliminación lógica del color.
        /// 
        /// Reglas de negocio:
        /// - No permite eliminar si está asociado a productos.
        /// - Cambia el estado Activo a false.
        /// - Mantiene integridad de datos histórica.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var color = await _context.Colores.FindAsync(id);

            if (color == null || !color.Activo)
                return NotFound();

            bool enUso = await _context.Productos
                .AnyAsync(p => p.ID_Color == id);

            if (enUso)
            {
                TempData["Error"] =
                    "No se puede eliminar el color porque está asociado a productos.";
                return RedirectToAction(nameof(Index));
            }

            color.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Color eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // RESTAURAR
        // ====================================================
        /// <summary>
        /// Restaura un color previamente eliminado de forma lógica.
        /// 
        /// Validaciones:
        /// - El registro debe existir.
        /// - Debe encontrarse inactivo.
        /// - No debe existir otro color activo con el mismo nombre.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restaurar(int id)
        {
            var color = await _context.Colores.FindAsync(id);

            if (color == null || color.Activo)
                return NotFound();

            var nombreNormalizado = color.Nombre.ToUpper();

            // Verifica que no exista conflicto de nombre con registros activos
            bool duplicado = await _context.Colores
                .AnyAsync(c =>
                    c.ID_Color != id &&
                    c.Activo &&
                    c.Nombre.ToUpper() == nombreNormalizado);

            if (duplicado)
            {
                TempData["Error"] =
                    "No se puede restaurar porque ya existe un color activo con ese nombre.";
                return RedirectToAction(nameof(Index));
            }

            color.Activo = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Color restaurado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
