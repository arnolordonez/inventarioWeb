using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventarioWEB.Models;
using System.Diagnostics;
using InventarioWEB;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador principal de la aplicación.
    /// </summary>
    /// <remarks>
    /// Gestiona las vistas públicas iniciales del sistema y el manejo
    /// centralizado de errores no controlados.
    /// 
    /// Aunque no pertenece directamente al módulo Productos,
    /// actúa como punto de entrada general de la aplicación web.
    /// </remarks>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del <see cref="HomeController"/>.
        /// </summary>
        /// <param name="logger">
        /// Instancia de logger utilizada para registrar eventos,
        /// advertencias o errores generados dentro del controlador.
        /// </param>
        /// <remarks>
        /// El logger es inyectado mediante el mecanismo de
        /// inyección de dependencias del framework ASP.NET Core.
        /// </remarks>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // ==========================================================
        // VISTA PRINCIPAL (Inicio)
        // ==========================================================

        /// <summary>
        /// Muestra la vista principal de la aplicación.
        /// </summary>
        /// <returns>
        /// Retorna la vista Index asociada al controlador.
        /// </returns>
        /// <remarks>
        /// Representa el punto de entrada visual del sistema.
        /// Desde esta vista se puede redirigir hacia módulos
        /// como Productos, Catálogos o Autenticación.
        /// </remarks>
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================================
        // VISTA DE PRIVACIDAD
        // ==========================================================

        /// <summary>
        /// Muestra la política de privacidad de la aplicación.
        /// </summary>
        /// <returns>
        /// Retorna la vista Privacy.
        /// </returns>
        /// <remarks>
        /// Vista estándar generada en proyectos ASP.NET Core MVC.
        /// Puede adaptarse a requisitos legales o normativos.
        /// </remarks>
        public IActionResult Privacy()
        {
            return View();
        }

        // ==========================================================
        // PÁGINA DE ERROR GLOBAL
        // ==========================================================

        /// <summary>
        /// Muestra la página de error global de la aplicación.
        /// </summary>
        /// <returns>
        /// Retorna la vista Error junto con el identificador
        /// único de la solicitud actual.
        /// </returns>
        /// <remarks>
        /// Se desactiva el almacenamiento en caché mediante
        /// el atributo ResponseCache para evitar mostrar errores
        /// almacenados previamente.
        /// 
        /// El identificador RequestId facilita el seguimiento
        /// y depuración de incidentes en ambientes productivos.
        /// </remarks>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
