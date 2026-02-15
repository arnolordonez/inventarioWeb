using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels.Catalogos.Tallas;

namespace InventarioWEB.Controllers.Catalogos
{
    /// <summary>
    /// Controlador encargado de gestionar el catálogo de Tallas.
    /// 
    /// Implementa operaciones CRUD con borrado lógico, validación de duplicidad
    /// por Género + Descripción, y verificación de integridad referencial con Productos.
    /// 
    /// Patrón aplicado:
    /// - Separación de responsabilidades mediante ViewModels
    /// - Uso de Entity Framework Core con consultas asincrónicas
    /// - Borrado lógico mediante bandera Activo
    /// </summary>
    public class TallasController : Controller
    {
        /// <summary>
        /// Contexto de base de datos inyectado mediante DI.
        /// Permite el acceso a las entidades Tallas, Generos y Productos.
        /// </summary>
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Constructor del controlador.
        /// </summary>
        /// <param name="context">Contexto de base de datos del sistema.</param>
        public TallasController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ====================================================
        // INDEX (SOLO ACTIVOS)
        // ====================================================

        /// <summary>
        /// Muestra el listado de tallas activas.
        /// 
        /// Características:
        /// - Solo consulta registros con Activo = true
        /// - Uso de AsNoTracking para optimización de lectura
        /// - Ordenamiento primero por Género y luego por ID
        /// - Proyección directa a ViewModel para desacoplar entidad
        /// </summary>
        /// <returns>Vista Index con colección tipada de tallas</returns>
        public async Task<IActionResult> Index()
        {
            var tallas = await _context.Tallas
                .AsNoTracking()
                .Where(t => t.Activo)
                .OrderBy(t => t.ID_Genero)      // 🔵 primero ordena por género
                .ThenBy(t => t.ID_Tallas)       // 🔵 luego por ID de talla

                .Select(t => new TallaItemVM
                {
                    ID_Tallas = t.ID_Tallas,



                    DescripTalla = t.DescripTalla,
                    Genero = t.Genero.DescripGenero
                })
                .ToListAsync();

            return View(new TallasIndexVM
            {
                Tallas = tallas
            });
        }

        // ====================================================
        // CREAR
        // ====================================================

        /// <summary>
        /// Muestra el formulario de creación de una nueva talla.
        /// 
        /// Se carga la lista de géneros para el componente SelectList.
        /// </summary>
        /// <returns>Vista Crear con modelo inicializado</returns>
        public async Task<IActionResult> Crear()
        {
            return View(new TallaCreateVM
            {
                Generos = await ObtenerGeneros()
            });
        }

        /// <summary>
        /// Procesa la creación de una nueva talla.
        /// 
        /// Validaciones implementadas:
        /// - Validación de modelo (DataAnnotations)
        /// - Validación de descripción no vacía
        /// - Control de duplicidad por (Genero + Descripción)
        /// 
        /// Aplica normalización en mayúsculas para comparación case-insensitive.
        /// </summary>
        /// <param name="model">Modelo enviado desde la vista</param>
        /// <returns>Redirección a Index o retorno a la vista con errores</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(TallaCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Generos = await ObtenerGeneros();
                return View(model);
            }

            var descripcion = model.DescripTalla.Trim();

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(nameof(model.DescripTalla),
                    "La descripción no es válida.");
                model.Generos = await ObtenerGeneros();
                return View(model);
            }

            var descripcionNormalizada = descripcion.ToUpper();

            bool existe = await _context.Tallas.AnyAsync(t =>
                t.Activo &&
                t.ID_Genero == model.ID_Genero &&
                t.DescripTalla.ToUpper() == descripcionNormalizada);

            if (existe)
            {
                ModelState.AddModelError(nameof(model.DescripTalla),
                    "Ya existe una talla con esa descripción para el género seleccionado.");
                model.Generos = await ObtenerGeneros();
                return View(model);
            }

            var talla = new Talla
            {
                DescripTalla = descripcion,
                ID_Genero = model.ID_Genero,
                Activo = true
            };

            _context.Tallas.Add(talla);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Talla creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // EDITAR
        // ====================================================

        /// <summary>
        /// Muestra el formulario de edición de una talla activa.
        /// 
        /// Utiliza AsNoTracking dado que el objeto será reconstruido
        /// posteriormente en el POST para actualización.
        /// </summary>
        /// <param name="id">Identificador de la talla</param>
        /// <returns>Vista Editar o NotFound</returns>
        public async Task<IActionResult> Editar(int id)
        {
            var talla = await _context.Tallas
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ID_Tallas == id && t.Activo);

            if (talla == null)
                return NotFound();

            return View(new TallaEditVM
            {
                ID_Tallas = talla.ID_Tallas,
                DescripTalla = talla.DescripTalla,
                ID_Genero = talla.ID_Genero,
                Generos = await ObtenerGeneros()
            });
        }

        /// <summary>
        /// Procesa la actualización de una talla existente.
        /// 
        /// Validaciones:
        /// - Coincidencia de ID ruta vs modelo
        /// - Validación de modelo
        /// - Control de duplicidad excluyendo el registro actual
        /// - Verificación de existencia y estado activo
        /// </summary>
        /// <param name="id">ID enviado por ruta</param>
        /// <param name="model">Modelo enviado desde vista</param>
        /// <returns>Redirección a Index o retorno con errores</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, TallaEditVM model)
        {
            if (id != model.ID_Tallas)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                model.Generos = await ObtenerGeneros();
                return View(model);
            }

            var talla = await _context.Tallas.FindAsync(id);

            if (talla == null || !talla.Activo)
                return NotFound();

            var descripcion = model.DescripTalla.Trim();

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(nameof(model.DescripTalla),
                    "La descripción no es válida.");
                model.Generos = await ObtenerGeneros();
                return View(model);
            }

            var descripcionNormalizada = descripcion.ToUpper();

            bool duplicado = await _context.Tallas.AnyAsync(t =>
                t.Activo &&
                t.ID_Tallas != id &&
                t.ID_Genero == model.ID_Genero &&
                t.DescripTalla.ToUpper() == descripcionNormalizada);

            if (duplicado)
            {
                ModelState.AddModelError(nameof(model.DescripTalla),
                    "Ya existe una talla con esa descripción para el género seleccionado.");
                model.Generos = await ObtenerGeneros();
                return View(model);
            }

            talla.DescripTalla = descripcion;
            talla.ID_Genero = model.ID_Genero;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Talla actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // ELIMINAR (GET CONFIRMACIÓN)
        // ====================================================

        /// <summary>
        /// Muestra la vista de confirmación para eliminación lógica.
        /// 
        /// Solo permite eliminación de registros activos.
        /// </summary>
        /// <param name="id">ID de la talla</param>
        /// <returns>Vista de confirmación o NotFound</returns>
        public async Task<IActionResult> Eliminar(int id)
        {
            var talla = await _context.Tallas
                .AsNoTracking()
                .Where(t => t.ID_Tallas == id && t.Activo)
                .Select(t => new TallaDeleteVM
                {
                    ID_Tallas = t.ID_Tallas,
                    DescripTalla = t.DescripTalla,
                    Genero = t.Genero.DescripGenero
                })
                .FirstOrDefaultAsync();

            if (talla == null)
                return NotFound();

            return View(talla);
        }

        // ====================================================
        // ELIMINAR (POST LÓGICO)
        // ====================================================

        /// <summary>
        /// Ejecuta el borrado lógico de una talla.
        /// 
        /// Reglas:
        /// - No se permite eliminar si está asociada a Productos
        /// - No se elimina físicamente el registro (soft delete)
        /// </summary>
        /// <param name="model">Modelo de confirmación</param>
        /// <returns>Redirección a Index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(TallaDeleteVM model)
        {
            var talla = await _context.Tallas.FindAsync(model.ID_Tallas);

            if (talla == null || !talla.Activo)
                return NotFound();

            bool enUso = await _context.Productos
                .AnyAsync(p => p.ID_Tallas == model.ID_Tallas);

            if (enUso)
            {
                TempData["Error"] =
                    "No se puede eliminar la talla porque está asociada a productos.";
                return RedirectToAction(nameof(Index));
            }

            talla.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Talla eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ====================================================
        // MÉTODO PRIVADO: SELECTLIST GÉNEROS
        // ====================================================

        /// <summary>
        /// Obtiene el listado de géneros ordenado por ID.
        /// 
        /// Se utiliza para poblar los SelectList en formularios de Crear y Editar.
        /// </summary>
        /// <returns>Lista de SelectListItem</returns>
        private async Task<List<SelectListItem>> ObtenerGeneros()
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
