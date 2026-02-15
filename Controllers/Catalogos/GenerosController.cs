using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels.Catalogos.Generos;

namespace InventarioWEB.Controllers.Catalogos
{
    /// <summary>
    /// Controlador encargado de la gestión del catálogo de Géneros.
    /// 
    /// Responsabilidades:
    /// - Listar los géneros registrados en el sistema.
    /// - Permitir la actualización de su descripción.
    /// 
    /// Características técnicas:
    /// - Implementa arquitectura MVC.
    /// - Utiliza Entity Framework Core para acceso a datos.
    /// - Emplea ViewModels para desacoplar las entidades del dominio.
    /// - Incluye validaciones de integridad y duplicidad.
    /// </summary>
    public class GenerosController : Controller
    {
        /// <summary>
        /// Contexto de base de datos utilizado para
        /// la interacción con las entidades del sistema.
        /// Inyectado mediante Dependency Injection.
        /// </summary>
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Constructor del controlador.
        /// Permite inicializar el acceso al contexto de datos.
        /// </summary>
        public GenerosController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ====================================================
        // INDEX (SOLO CONSULTA)
        // ====================================================
        /// <summary>
        /// Muestra el listado completo de géneros.
        /// 
        /// Funcionalidad:
        /// - Consulta todos los registros existentes.
        /// - Aplica ordenamiento por identificador primario (ID_Genero).
        /// - Utiliza AsNoTracking() para optimizar rendimiento en consultas de solo lectura.
        /// - Proyecta los datos hacia un ViewModel.
        /// </summary>
        /// <returns>
        /// Vista con el listado de géneros registrados.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            var generos = await _context.Generos
                .AsNoTracking()
                .OrderBy(g => g.ID_Genero)
                .Select(g => new GeneroItemVM
                {
                    ID_Genero = g.ID_Genero,
                    DescripGenero = g.DescripGenero
                })
                .ToListAsync();

            return View(new GeneroIndexVM
            {
                Generos = generos
            });
        }

        // ====================================================
        // EDITAR
        // ====================================================
        /// <summary>
        /// Carga la vista de edición de un género específico.
        /// 
        /// Validaciones:
        /// - Verifica la existencia del registro en base de datos.
        /// </summary>
        /// <param name="id">Identificador del género a editar.</param>
        /// <returns>
        /// Vista de edición con los datos actuales del género.
        /// </returns>
        public async Task<IActionResult> Editar(int id)
        {
            var genero = await _context.Generos
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.ID_Genero == id);

            if (genero == null)
                return NotFound();

            return View(new GeneroEditVM
            {
                ID_Genero = genero.ID_Genero,
                DescripGenero = genero.DescripGenero
            });
        }

        /// <summary>
        /// Procesa la actualización de un género existente.
        /// 
        /// Reglas de validación:
        /// - Coincidencia entre parámetro URL y modelo recibido.
        /// - Modelo válido según anotaciones de datos.
        /// - Existencia del registro.
        /// - Descripción no vacía.
        /// - Control de duplicidad insensible a mayúsculas/minúsculas.
        /// </summary>
        /// <param name="id">Identificador del género.</param>
        /// <param name="model">Modelo de edición recibido desde la vista.</param>
        /// <returns>
        /// Redirección al listado en caso de éxito o recarga de vista si hay errores.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, GeneroEditVM model)
        {
            // Validación de integridad de la solicitud
            if (id != model.ID_Genero)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var genero = await _context.Generos.FindAsync(id);
            if (genero == null)
                return NotFound();

            var descripcion = model.DescripGenero?.Trim();

            // Validación de descripción válida
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(nameof(model.DescripGenero),
                    "La descripción no es válida.");
                return View(model);
            }

            var descripcionNormalizada = descripcion.ToUpper();

            // Verifica si existe otro género con la misma descripción
            bool duplicado = await _context.Generos
                .AnyAsync(g =>
                    g.ID_Genero != id &&
                    g.DescripGenero.ToUpper() == descripcionNormalizada);

            if (duplicado)
            {
                ModelState.AddModelError(nameof(model.DescripGenero),
                    "Ya existe un género con esa descripción.");
                return View(model);
            }

            // Actualización del registro
            genero.DescripGenero = descripcion;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Género actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
