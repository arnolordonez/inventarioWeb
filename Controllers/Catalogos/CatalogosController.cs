using Microsoft.AspNetCore.Mvc;

namespace InventarioWEB.Controllers.Catalogos
{
    /// <summary>
    /// Controlador principal del módulo de Catálogos.
    /// 
    /// Responsabilidad:
    /// Actuar como punto de entrada al submódulo de administración
    /// de catálogos del sistema (Tallas, Telas, Referencias, Colores, etc.).
    /// 
    /// Este controlador no contiene lógica de negocio directa;
    /// únicamente enruta a la vista principal del módulo.
    /// 
    /// Forma parte de la arquitectura MVC del proyecto InventarioWEB.
    /// </summary>
    public class CatalogosController : Controller
    {
        // ====================================================
        // MÉTODO: Index
        // RUTA: /Catalogos/Index
        // ====================================================
        /// <summary>
        /// Muestra la vista principal del módulo de Catálogos.
        /// 
        /// Funcionalidad:
        /// - Renderiza la vista inicial del módulo.
        /// - Permite al usuario acceder a las diferentes
        ///   secciones de catálogos disponibles en el sistema.
        /// 
        /// Tipo de respuesta:
        /// IActionResult → Retorna una vista MVC.
        /// 
        /// Observaciones técnicas:
        /// - No interactúa con base de datos.
        /// - No requiere parámetros.
        /// - No implementa validaciones.
        /// </summary>
        /// <returns>
        /// Vista correspondiente al módulo de Catálogos.
        /// </returns>
        public IActionResult Index()
        {
            // Retorna la vista por convención: Views/Catalogos/Index.cshtml
            return View();
        }
    }
}
