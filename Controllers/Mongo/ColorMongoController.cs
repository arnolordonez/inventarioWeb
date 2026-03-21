using Microsoft.AspNetCore.Mvc;
using InventarioWEB.Mongo.Services;
using InventarioWEB.Mongo.Models;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador para la gestión de colores en MongoDB.
    /// Maneja CRUD con eliminación lógica y restauración.
    /// </summary>
    public class ColorMongoController : Controller
    {
        private readonly ColorService _service;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ColorMongoController(ColorService service)
        {
            _service = service;
        }

        // =========================
        // LISTA + FILTRO
        // =========================

        /// <summary>
        /// Muestra la lista de colores activos.
        /// Permite filtrar por nombre.
        /// </summary>
        public async Task<IActionResult> Index(string filtro)
        {
            var colores = await _service.GetAllAsync();

            // 🔍 Filtro por nombre
            if (!string.IsNullOrEmpty(filtro))
            {
                colores = colores
                    .Where(c => c.Nombre.ToLower().Contains(filtro.ToLower()))
                    .ToList();
            }

            ViewBag.Filtro = filtro;

            return View(colores);
        }

        // =========================
        // CREAR
        // =========================

        /// <summary>
        /// Muestra el formulario de creación
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Procesa la creación de un nuevo color
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ColorMongo color)
        {
            ModelState.Remove(nameof(color.Id));

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Error al guardar el color";
                return View(color);
            }

            color.Activo = true;

            await _service.CreateAsync(color);

            // ✅ Toast éxito
            TempData["success"] = "Color creado correctamente";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDITAR
        // =========================

        /// <summary>
        /// Muestra el formulario de edición
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var color = await _service.GetByIdAsync(id);

            if (color == null)
                return NotFound();

            return View(color);
        }

        /// <summary>
        /// Procesa la actualización de un color
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ColorMongo color)
        {
            if (id != color.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Error al actualizar el color";
                return View(color);
            }

            await _service.UpdateAsync(id, color);

            TempData["success"] = "Color actualizado correctamente";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // ELIMINACIÓN LÓGICA
        // =========================

        /// <summary>
        /// Desactiva un color (eliminación lógica)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            await _service.DeleteAsync(id);

            TempData["success"] = "Color desactivado correctamente";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // INACTIVOS
        // =========================

        /// <summary>
        /// Lista los colores inactivos
        /// </summary>
        public async Task<IActionResult> Inactivos()
        {
            var colores = await _service.GetInactivosAsync();
            return View(colores);
        }

        // =========================
        // RESTAURAR
        // =========================

        /// <summary>
        /// Reactiva un color previamente eliminado
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            await _service.RestoreAsync(id);

            TempData["success"] = "Color recuperado correctamente";

            return RedirectToAction(nameof(Inactivos));
        }
    }
}