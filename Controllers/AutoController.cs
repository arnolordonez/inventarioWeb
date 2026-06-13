using Microsoft.AspNetCore.Mvc;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.Models.DTO;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.ViewModels;

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
        // ==========================
        // CONTEXTOS
        // ==========================
        private readonly UsuariosDbContext _usuariosContext;
        private readonly MovimientoVentasDbContext _ventasContext;

        // ==========================
        // CONSTRUCTOR
        // ==========================
        public AutoController(
            UsuariosDbContext usuariosContext,
            MovimientoVentasDbContext ventasContext)
        {
            _usuariosContext = usuariosContext;
            _ventasContext = ventasContext;
        }


        /*
        public class AutoController : Controller
        {
        private readonly MovimientoVentasDbContext _context;

        public AutoController(MovimientoVentasDbContext context)
        {
            _context = context;
        }


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
              */


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
            ViewBag.Correos = _usuariosContext.Usuarios
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
            if (!ModelState.IsValid)
                return View(request);

            // ==========================================
            // VALIDAR USUARIO INACTIVO
            // ==========================================
           

            // ==========================================
            // BUSCAR USUARIO ACTIVO CON SU ROL
            // ==========================================
            var usuario = _usuariosContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u =>
                    u.Correo == request.Correo &&
                    u.Activo);

            // ==========================================
            // VALIDAR CONTRASEÑA
            // ==========================================
            if (usuario != null &&
                BCrypt.Net.BCrypt.Verify(
                    request.Contrasena,
                    usuario.HashContrasena))
            {
                // ==========================================
                // SESIÓN ERP
                // ==========================================
                HttpContext.Session.SetString(
                    "UsuarioID",
                    usuario.IdUsuario.ToString());

                HttpContext.Session.SetString(
                    "UsuarioNombre",
                    $"{usuario.Nombres} {usuario.Apellidos}");

                HttpContext.Session.SetString(
                    "Rol",
                    usuario.Rol?.NombreRol ?? string.Empty);

                HttpContext.Session.SetString(
                    "IdRol",
                    usuario.IdRol.ToString());

                return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError(
                "",
                "Correo o contraseña incorrectos.");

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
        public async Task<IActionResult> Dashboard()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioID");

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login");

            var hoy = DateTime.Today;

            /*
            var usuarioId = HttpContext.Session.GetString("UsuarioID");

            Console.WriteLine($"UsuarioID sesión: {usuarioId}");

            if (string.IsNullOrEmpty(usuarioId))
            {
                Console.WriteLine("⚠️ Sesión expirada. Redireccionando a Login.");
                return RedirectToAction("Login");
            }

            var hoy = DateTime.Today;
            */
            // ==========================
            // 💰 VENTAS HOY
            // ==========================

            // Rango del día (correcto para EF + MySQL)
            var inicioDia = DateTime.Today;
            var finDia = inicioDia.AddDays(1);

            // Obtener IDs de pedidos despachados hoy
            var pedidosIds = await _ventasContext.Despachos
                .Where(d => d.Estado == EstadoDespacho.Despachado
                    && d.Fecha >= inicioDia
                    && d.Fecha < finDia)
                .Select(d => d.ID_Pedido)
                .ToListAsync();

            // Total de despachos
            var totalDespachos = pedidosIds.Count;

            // Suma de ventas
            var ventasHoy = await _ventasContext.Pedidos
                .Where(p => pedidosIds.Contains(p.ID_Pedido))
                .SumAsync(p => (decimal?)p.TotalVenta) ?? 0;


            // ==========================
            // 📦 STOCK TOTAL
            // ==========================
            var stockTotal = await _ventasContext.Productos
                .Where(p => p.Activo)
                .SumAsync(p => (int?)p.Stock) ?? 0;


            // ==========================
            // ⚠ STOCK BAJO
            // ==========================
            var stockBajo = await _ventasContext.Productos
                .Where(p => p.Activo && p.Stock <= 10)
                .CountAsync();


            // ==========================
            // 🧵 PRODUCCIÓN ACTIVA
            // ==========================
            var produccionActiva = await _ventasContext.Producciones
                .Where(p => p.Activo)
                .CountAsync();


            // ==========================
            // 📊 VIEWMODEL
            // ==========================
            var model = new DashboardViewModel
            {
                VentasHoy = ventasHoy,
                TotalDespachos = totalDespachos,
                StockTotal = stockTotal,
                StockBajo = stockBajo,
                ProduccionActiva = produccionActiva
            };

            ViewBag.NombreUsuario = HttpContext.Session.GetString("UsuarioNombre");

            return View(model);
        }


        // ==========================================================
        // REGISTRO DE USUARIO (DESHABILITADO)
        // ==========================================================

        [HttpGet]
        public IActionResult Register()
        {
            TempData["error"] =
                "El registro de usuarios se encuentra deshabilitado.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Register(RegisterRequest request)
        {
            // ==========================================================
            // REGISTRO PÚBLICO DESHABILITADO
            // ERP: Los usuarios deben ser creados únicamente
            // por un Administrador del sistema.
            // ==========================================================

            TempData["error"] =
                "El registro de usuarios se encuentra deshabilitado. Solicite la creación de su cuenta al Administrador del sistema.";

            return RedirectToAction("Login");
        }


        // ==========================================================
        // LOGOUT
        // ==========================================================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ==========================================================
        // ACCESO DENEGADO
        // ==========================================================

        public IActionResult AccesoDenegado()
        {
            return View();
        }


        // ==========================================================
        // FORGOT PASSWORD (GET)
        // ==========================================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequest());
        }


        // ==========================================================
        // FORGOT PASSWORD (POST)
        // ==========================================================

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var usuario = _usuariosContext.Usuarios
                .FirstOrDefault(u => u.Correo == request.Correo);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo no registrado.");
                return View(request);
            }

            var token = Guid.NewGuid().ToString();

            HttpContext.Session.SetString("ResetToken", token);
            HttpContext.Session.SetString("ResetUserId", usuario.IdUsuario.ToString());

            ViewBag.Mensaje =
                $"Token generado: {token} (en producción se enviaría por correo).";

            return View(request);
        }


        // ==========================================================
        // RESET PASSWORD (GET)
        // ==========================================================

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

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var sessionToken = HttpContext.Session.GetString("ResetToken");
            var userIdStr = HttpContext.Session.GetString("ResetUserId");

            if (sessionToken != request.Token || string.IsNullOrEmpty(userIdStr))
            {
                ModelState.AddModelError("", "Token inválido o expirado.");
                return View(request);
            }

            int userId = int.Parse(userIdStr);

            var usuario = _usuariosContext.Usuarios
                .FirstOrDefault(u => u.IdUsuario == userId);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View(request);
            }

            usuario.HashContrasena =
                BCrypt.Net.BCrypt.HashPassword(request.NuevaContrasena.Trim());

            usuario.FechaUltimaActualizacion = DateTime.Now;

            _usuariosContext.SaveChanges();

            HttpContext.Session.Remove("ResetToken");
            HttpContext.Session.Remove("ResetUserId");

            return RedirectToAction("Login");
        }


        // ==========================================================
        // MODELO FORGOT PASSWORD
        // ==========================================================

        public class ForgotPasswordRequest
        {
            [Required(ErrorMessage = "El correo es obligatorio.")]
            [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
            public string Correo { get; set; } = string.Empty;
        }
    }
}
