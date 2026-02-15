using Microsoft.AspNetCore.Mvc;
using InventarioWEB.Data;
using System;
using System.Linq;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador de diagnóstico para validación de conectividad con la base de datos.
    /// 
    /// Propósito:
    /// - Verificar que la inyección de dependencias del contexto funcione correctamente.
    /// - Confirmar conectividad con el motor MySQL.
    /// - Validar acceso a la tabla Usuarios.
    /// 
    /// Este controlador no forma parte del flujo productivo del sistema.
    /// Debe utilizarse únicamente para pruebas técnicas y diagnóstico.
    /// </summary>
    public class TestController : Controller
    {
        private readonly UsuariosDbContext _context;

        /// <summary>
        /// Constructor del controlador de pruebas.
        /// 
        /// Implementa inyección de dependencias del contexto
        /// <see cref="UsuariosDbContext"/>.
        /// </summary>
        /// <param name="context">
        /// Contexto de base de datos encargado de la gestión
        /// de la entidad Usuarios.
        /// </param>
        public TestController(UsuariosDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ejecuta una prueba básica de conectividad contra la base de datos.
        /// 
        /// Procedimiento:
        /// 1. Intenta contar los registros de la tabla Usuarios.
        /// 2. Si la operación es exitosa, confirma conexión válida.
        /// 3. Si ocurre una excepción, retorna el mensaje de error para diagnóstico.
        /// 
        /// Nota:
        /// Este método no aplica lógica de negocio ni validaciones adicionales.
        /// </summary>
        /// <returns>
        /// Resultado textual indicando éxito o fallo en la conexión.
        /// </returns>
        /// <exception cref="Exception">
        /// Puede lanzar excepción si la conexión, el contexto o la consulta fallan.
        /// </exception>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var total = _context.Usuarios.Count();

                return Content(
                    $"✅ Conexión correcta con la base de datos MySQL.\n" +
                    $"Usuarios registrados en la tabla: {total}"
                );
            }
            catch (Exception ex)
            {
                return Content(
                    $"❌ ERROR al conectar con la base de datos MySQL:\n{ex.Message}"
                );
            }
        }
    }
}
