using Microsoft.AspNetCore.Mvc;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.Models.DTO;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador responsable de la autenticación, registro y gestión de sesión
    /// de los usuarios del sistema.
    /// </summary>
    /// <remarks>
    /// Interactúa directamente con la entidad Usuario a través de UsuariosDbContext.
    /// Gestiona procesos de inicio de sesión, cierre de sesión,
    /// registro de usuarios y recuperación de contraseña.
    /// Aunque no pertenece al módulo Productos, actúa de forma indirecta
    /// asegurando el acceso controlado a dicho módulo.
    /// </remarks>
    public class AutoController : Controller
    {
        private readonly UsuariosDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de autenticación.
        /// </summary>
        /// <param name="context">
        /// Contexto de base de datos utilizado para acceder a la tabla Usuarios.
        /// </param>
        public AutoController(UsuariosDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // LOGIN (GET)
        // ==========================================================

        /// <summary>
        /// Muestra la vista de inicio de sesión del sistema.
        /// </summary>
        /// <returns>
        /// Retorna la vista Login junto con los últimos cinco correos activos registrados.
        /// </returns>
        /// <remarks>
        /// Se cargan los cinco correos más recientes con el fin de facilitar el acceso al usuario.
        /// </remarks>
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Correos = _context.Usuarios
                                      .Where(u => u.Activo)
                                      .OrderByDescending(u => u.FechaCreacion)
                                      .Select(u => u.Correo)
                                      .Take(5)
                                      .ToList();

            return View(new LoginRequest());
        }

        // ==========================================================
        // LOGIN (POST)
        // ==========================================================

        /// <summary>
        /// Procesa las credenciales enviadas por el usuario para iniciar sesión.
        /// </summary>
        /// <param name="request">
        /// Modelo que contiene el correo y la contraseña ingresados.
        /// </param>
        /// <returns>
        /// Redirecciona al Dashboard si las credenciales son válidas;
        /// en caso contrario retorna la vista Login con mensajes de error.
        /// </returns>
        /// <remarks>
        /// La contraseña es validada utilizando el algoritmo BCrypt.
        /// En caso exitoso, se almacenan identificadores básicos en la sesión HTTP.
        /// </remarks>
        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == request.Correo);
            if (usuario != null && BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.HashContrasena))
            {
                HttpContext.Session.SetString("UsuarioID", usuario.IdUsuario.ToString());
                HttpContext.Session.SetString("UsuarioNombre", $"{usuario.Nombres} {usuario.Apellidos}");
                return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(request);
        }

        // ==========================================================
        // DASHBOARD
        // ==========================================================

        /// <summary>
        /// Muestra el panel principal del sistema.
        /// </summary>
        /// <returns>
        /// Retorna la vista Dashboard si existe sesión activa;
        /// de lo contrario redirecciona al Login.
        /// </returns>
        /// <remarks>
        /// Requiere que la variable de sesión "UsuarioID" esté definida.
        /// </remarks>
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioID")))
                return RedirectToAction("Login");

            ViewBag.NombreUsuario = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }

        // ==========================================================
        // REGISTRO DE USUARIO
        // ==========================================================

        /// <summary>
        /// Muestra el formulario para registrar un nuevo usuario.
        /// </summary>
        /// <returns>
        /// Retorna la vista Register.
        /// </returns>
        [HttpGet]
        public IActionResult Register() => View();

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="request">
        /// Modelo que contiene la información del usuario a registrar.
        /// </param>
        /// <returns>
        /// Redirecciona a Login si el registro es exitoso;
        /// en caso contrario retorna la vista con errores de validación.
        /// </returns>
        /// <remarks>
        /// Se valida que el correo no exista previamente.
        /// La contraseña es cifrada utilizando BCrypt antes de almacenarse.
        /// </remarks>
        [HttpPost]
        public IActionResult Register(RegisterRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            if (_context.Usuarios.Any(u => u.Correo == request.Correo))
            {
                ModelState.AddModelError("Correo", "El correo ya está registrado.");
                return View(request);
            }

            var nuevoUsuario = new Usuario
            {
                Nombres = request.Nombres.Trim(),
                Apellidos = request.Apellidos.Trim(),
                Correo = request.Correo.Trim(),
                Salt = Guid.NewGuid().ToString(),
                HashContrasena = BCrypt.Net.BCrypt.HashPassword(request.Contrasena.Trim()),
                IdRol = 1,
                Activo = true,
                FechaCreacion = DateTime.Now,
                FechaUltimaActualizacion = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // ==========================================================
        // LOGOUT
        // ==========================================================

        /// <summary>
        /// Finaliza la sesión activa del usuario.
        /// </summary>
        /// <returns>
        /// Redirecciona a la vista Login.
        /// </returns>
        /// <remarks>
        /// Elimina todas las variables almacenadas en la sesión HTTP.
        /// </remarks>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ==========================================================
        // FORGOT PASSWORD (GET)
        // ==========================================================

        /// <summary>
        /// Muestra el formulario para solicitar recuperación de contraseña.
        /// </summary>
        /// <returns>
        /// Retorna la vista ForgotPassword.
        /// </returns>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequest());
        }

        // ==========================================================
        // FORGOT PASSWORD (POST)
        // ==========================================================

        /// <summary>
        /// Genera un token temporal para el restablecimiento de contraseña.
        /// </summary>
        /// <param name="request">
        /// Modelo que contiene el correo del usuario solicitante.
        /// </param>
        /// <returns>
        /// Retorna la vista mostrando mensaje informativo o errores.
        /// </returns>
        /// <remarks>
        /// En un entorno productivo el token debería enviarse por correo electrónico.
        /// En esta implementación el token se almacena en sesión como ejemplo funcional.
        /// </remarks>
        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == request.Correo);
            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo no registrado.");
                return View(request);
            }

            var token = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("ResetToken", token);
            HttpContext.Session.SetString("ResetUserId", usuario.IdUsuario.ToString());

            ViewBag.Mensaje = $"Token generado: {token} (en producción se enviaría por correo).";
            return View(request);
        }

        // ==========================================================
        // RESET PASSWORD (GET)
        // ==========================================================

        /// <summary>
        /// Muestra el formulario para establecer una nueva contraseña.
        /// </summary>
        /// <param name="token">
        /// Token de validación generado previamente.
        /// </param>
        /// <returns>
        /// Retorna la vista ResetPassword con el token precargado.
        /// </returns>
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var model = new ResetPasswordRequest
            {
                Token = token ?? string.Empty
            };
            return View(model);
        }

        // ==========================================================
        // RESET PASSWORD (POST)
        // ==========================================================

        /// <summary>
        /// Actualiza la contraseña del usuario validando el token almacenado en sesión.
        /// </summary>
        /// <param name="request">
        /// Modelo que contiene el token y la nueva contraseña.
        /// </param>
        /// <returns>
        /// Redirecciona al Login si la actualización es exitosa;
        /// en caso contrario retorna la vista con errores.
        /// </returns>
        /// <remarks>
        /// Una vez validado el token, se actualiza el hash de la contraseña
        /// y se eliminan los datos temporales de la sesión.
        /// </remarks>
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var sessionToken = HttpContext.Session.GetString("ResetToken");
            var userIdStr = HttpContext.Session.GetString("ResetUserId");

            if (sessionToken != request.Token || string.IsNullOrEmpty(userIdStr))
            {
                ModelState.AddModelError("", "Token inválido o expirado.");
                return View(request);
            }

            int userId = int.Parse(userIdStr);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == userId);
            if (usuario == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View(request);
            }

            usuario.HashContrasena = BCrypt.Net.BCrypt.HashPassword(request.NuevaContrasena.Trim());
            usuario.FechaUltimaActualizacion = DateTime.Now;
            _context.SaveChanges();

            HttpContext.Session.Remove("ResetToken");
            HttpContext.Session.Remove("ResetUserId");

            ViewBag.Mensaje = "Contraseña actualizada con éxito. Puede iniciar sesión.";
            return RedirectToAction("Login");
        }
    }

    /// <summary>
    /// Modelo utilizado para solicitar la recuperación de contraseña.
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// Correo electrónico del usuario que solicita el restablecimiento.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; } = string.Empty;
    }
}
