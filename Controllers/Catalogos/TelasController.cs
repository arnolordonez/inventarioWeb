using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels.Catalogos.Telas;

namespace InventarioWEB.Controllers.Catalogos
{
    /// <summary>
    /// Controlador encargado de la gestión del catálogo de Telas.
    /// 
    /// Implementa operaciones CRUD con:
    /// - Borrado lógico (soft delete)
    /// - Restauración de registros eliminados
    /// - Validación de duplicidad por descripción normalizada
    /// - Validación de integridad referencial con ReferenciasTelas
    /// 
    /// Arquitectura aplicada:
    /// - Separación mediante ViewModels
    /// - Uso de Entity Framework Core asincrónico
    /// - Normalización centralizada de descripciones
    /// </summary>
    public class TelasController : Controller
    {
        /// <summary>
        /// Contexto de base de datos inyectado mediante el contenedor de dependencias.
        /// Provee acceso a las entidades Telas y ReferenciasTelas.
        /// </summary>
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Constructor del controlador.
        /// </summary>
        /// <param name="context">Instancia del contexto de base de datos.</param>
        public TelasController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ====================================================
        // INDEX (ACTIVOS + ELIMINADOS)
        // ====================================================

        /// <summary>
        /// Muestra el listado completo de telas (activas y eliminadas).
        /// 
        /// Estrategia:
        /// - Consulta única a base de datos (optimización de acceso)
        /// - Separación en memoria entre registros activos y eliminados
        /// - Uso de AsNoTracking para consultas de solo lectura
        /// </summary>
        /// <returns>Vista Index con listas diferenciadas</returns>
        public async Task<IActionResult> Index()
        {
            var telas = await _context.Telas
                .AsNoTracking()
                .OrderBy(t => t.DescripTela)
                .Select(t => new
                {
                    t.ID_Telas,
                    t.DescripTela,
                    t.Activo
                })
                .ToListAsync();

            var vm = new TelasIndexVM
            {
                Telas = telas
                    .Where(t => t.Activo)
                    .Select(t => new TelaItemVM
                    {
                        ID_Telas = t.ID_Telas,
                        DescripTela = t.DescripTela
                    })
                    .ToList(),

                TelasEliminadas = telas
                    .Where(t => !t.Activo)
                    .Select(t => new TelaItemVM
                    {
                        ID_Telas = t.ID_Telas,
                        DescripTela = t.DescripTela
                    })
                    .ToList()
            };

            return View(vm);
        }

        // ====================================================
        // CREAR
        // ====================================================

        /// <summary>
        /// Muestra el formulario de creación de una nueva tela.
        /// </summary>
        /// <returns>Vista Crear</returns>
        public IActionResult Crear()
        {
            return View(new TelaCreateVM());
        }

        /// <summary>
        /// Procesa la creación de una nueva tela.
        /// 
        /// Validaciones:
        /// - Validación de modelo (DataAnnotations)
        /// - Normalización de descripción
        /// - Control de duplicidad sobre registros activos
        /// 
        /// La descripción se guarda normalizada (mayúsculas y sin espacios laterales)
        /// para mantener consistencia y evitar duplicados por formato.
        /// </summary>
        /// <param name="model">Modelo enviado desde la vista</param>
        /// <returns>Redirección a Index o retorno con errores</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(TelaCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var descripcion = NormalizarDescripcion(model.DescripTela);

            if (descripcion == null)
            {
                ModelState.AddModelError(nameof(model.DescripTela),
                    "La descripción no es válida.");
                return View(model);
            }

            bool existe = await _context.Telas
                .AnyAsync(t => t.Activo && t.DescripTela == descripcion);

            if (existe)
            {
                ModelState.AddModelError(nameof(model.DescripTela),
                    "Ya existe una tela con esa descripción.");
                return View(model);
            }

            var tela = new Tela
            {
                DescripTela = descripcion,
                Activo = true
            };

            _context.Telas.Add(tela);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tela creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // EDITAR
        // ====================================================

        /// <summary>
        /// Muestra el formulario de edición de una tela activa.
        /// 
        /// Usa AsNoTracking porque la actualización se realizará
        /// posteriormente con una nueva instancia recuperada por FindAsync.
        /// </summary>
        /// <param name="id">Identificador de la tela</param>
        /// <returns>Vista Editar o NotFound</returns>
        public async Task<IActionResult> Editar(int id)
        {
            var tela = await _context.Telas
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ID_Telas == id && t.Activo);

            if (tela == null)
                return NotFound();

            return View(new TelaEditVM
            {
                ID_Telas = tela.ID_Telas,
                DescripTela = tela.DescripTela
            });
        }

        /// <summary>
        /// Procesa la actualización de una tela existente.
        /// 
        /// Validaciones:
        /// - Coincidencia del ID de ruta con el del modelo
        /// - Validación de modelo
        /// - Normalización de descripción
        /// - Control de duplicidad excluyendo el registro actual
        /// - Verificación de que el registro esté activo
        /// </summary>
        /// <param name="id">ID enviado por ruta</param>
        /// <param name="model">Modelo enviado desde la vista</param>
        /// <returns>Redirección a Index o retorno con errores</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, TelaEditVM model)
        {
            if (id != model.ID_Telas)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var tela = await _context.Telas.FindAsync(id);

            if (tela == null || !tela.Activo)
                return NotFound();

            var descripcion = NormalizarDescripcion(model.DescripTela);

            if (descripcion == null)
            {
                ModelState.AddModelError(nameof(model.DescripTela),
                    "La descripción no es válida.");
                return View(model);
            }

            bool duplicado = await _context.Telas
                .AnyAsync(t =>
                    t.Activo &&
                    t.ID_Telas != id &&
                    t.DescripTela == descripcion);

            if (duplicado)
            {
                ModelState.AddModelError(nameof(model.DescripTela),
                    "Ya existe una tela con esa descripción.");
                return View(model);
            }

            tela.DescripTela = descripcion;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Tela actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // ELIMINAR (SOFT DELETE)
        // ====================================================

        /// <summary>
        /// Realiza el borrado lógico de una tela.
        /// 
        /// Reglas:
        /// - Solo puede eliminarse si está activa
        /// - No puede eliminarse si tiene relación en ReferenciasTelas
        /// - No se elimina físicamente el registro (soft delete)
        /// </summary>
        /// <param name="id">Identificador de la tela</param>
        /// <returns>Redirección a Index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var tela = await _context.Telas.FindAsync(id);

            if (tela == null || !tela.Activo)
                return NotFound();

            bool enUso = await _context.ReferenciasTelas
                .AnyAsync(rt => rt.ID_Telas == id);

            if (enUso)
            {
                TempData["Error"] =
                    "No se puede eliminar la tela porque está asociada a referencias.";
                return RedirectToAction(nameof(Index));
            }

            tela.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Tela eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // RESTAURAR
        // ====================================================

        /// <summary>
        /// Restaura una tela previamente eliminada.
        /// 
        /// Validaciones:
        /// - El registro debe existir y estar inactivo
        /// - No debe existir otra tela activa con la misma descripción
        /// 
        /// Garantiza consistencia funcional con la regla de unicidad.
        /// </summary>
        /// <param name="id">Identificador de la tela</param>
        /// <returns>Redirección a Index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restaurar(int id)
        {
            var tela = await _context.Telas.FindAsync(id);

            if (tela == null || tela.Activo)
                return NotFound();

            // Validar que no exista otra activa con misma descripción
            bool duplicado = await _context.Telas
                .AnyAsync(t =>
                    t.Activo &&
                    t.DescripTela == tela.DescripTela);

            if (duplicado)
            {
                TempData["Error"] =
                    "No se puede restaurar porque ya existe una tela activa con la misma descripción.";
                return RedirectToAction(nameof(Index));
            }

            tela.Activo = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Tela restaurada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // MÉTODO PRIVADO DE APOYO
        // ====================================================

        /// <summary>
        /// Normaliza la descripción de una tela.
        /// 
        /// Reglas:
        /// - Elimina espacios laterales
        /// - Convierte a mayúsculas
        /// - Devuelve null si la entrada es inválida
        /// 
        /// Centraliza la lógica de formato para mantener consistencia
        /// en todas las operaciones del controlador.
        /// </summary>
        /// <param name="descripcion">Texto recibido desde vista</param>
        /// <returns>Texto normalizado o null</returns>
        private string? NormalizarDescripcion(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return null;

            return descripcion.Trim().ToUpper();
        }
    }
}
