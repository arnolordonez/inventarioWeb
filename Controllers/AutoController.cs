using Microsoft.AspNetCore.Mvc;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.Models.DTO;
using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;
using InventarioWEB.ViewModels;
using InventarioWEB.Services.Interfaces;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador encargado de la autenticación y gestión de sesión
    /// de los usuarios del sistema (tabla Usuarios).
    /// </summary>
    public class AutoController : Controller
    {
        private readonly UsuariosDbContext _context;
        private readonly MovimientoVentasDbContext _movimientoVentasContext;
        private readonly TenantContext _tenantContext;
        private readonly IPlatformAccessService _platformAccessService;
        private readonly ITenantDbContextFactory _tenantDbContextFactory;

        public AutoController(
            UsuariosDbContext context,
            MovimientoVentasDbContext movimientoVentasContext,
            TenantContext tenantContext,
            IPlatformAccessService platformAccessService,
            ITenantDbContextFactory tenantDbContextFactory)
        {
            _context = context;
            _movimientoVentasContext = movimientoVentasContext;
            _tenantContext = tenantContext;
            _platformAccessService = platformAccessService;
            _tenantDbContextFactory = tenantDbContextFactory;
        }

        // ==========================================================
        // LOGIN (GET) - ahora con últimos 5 correos
        // ==========================================================
        [HttpGet]
        public IActionResult Login()
        {
            // Obtener los últimos 5 correos activos (más recientes)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginRequest request,
            string? slugEmpresa)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            // ======================================================
            // VALIDAR TENANT RESUELTO
            // ======================================================

            if (!_tenantContext.EstaResuelto ||
                !_tenantContext.IdEmpresa.HasValue)
            {
                ModelState.AddModelError(
                    "",
                    "No existe un Tenant seleccionado para esta sesión.");

                return View(request);
            }

            var idEmpresa =
                _tenantContext.IdEmpresa.Value;

            // ======================================================
            // AUTENTICAR USUARIO
            // ======================================================
            await using var tenantContext =
                _tenantDbContextFactory.CreateDbContext();

            var usuario =
                await tenantContext.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u =>
                        u.Correo == request.Correo &&
                        u.Activo);


            if (usuario == null ||
                !BCrypt.Net.BCrypt.Verify(
                    request.Contrasena,
                    usuario.HashContrasena))
            {
                ModelState.AddModelError(
                    "",
                    "Correo o contraseña incorrectos.");

                return View(request);
            }

            // ======================================================
            // VALIDAR AUTORIZACIÓN COMERCIAL
            // ======================================================

            var acceso =
                await _platformAccessService
                    .ValidarAccesoAsync(idEmpresa);

            if (!acceso.RespuestaValida)
            {
                ModelState.AddModelError(
                    "",
                    acceso.Motivo);

                return View(request);
            }

            if (!acceso.Permitido)
            {
                return RedirectToAction(
                    nameof(AccesoDenegado),
                    new
                    {
                        motivo = acceso.Motivo
                    });
            }

            // ======================================================
            // DATOS DE SESIÓN DEL USUARIO
            // ======================================================

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

            
            // ======================================================
            // TENANT DE LA SESIÓN
            // ======================================================

            HttpContext.Session.SetString(
                "IdEmpresa",
                idEmpresa.ToString());

            if (!string.IsNullOrWhiteSpace(
                slugEmpresa))
            {
                HttpContext.Session.SetString(
                    "SlugEmpresa",
                    slugEmpresa);
            }

            // ======================================================
            // ACCESO AL ERP
            // ======================================================
            if (string.IsNullOrWhiteSpace(slugEmpresa))
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo determinar la empresa de esta sesión.");

                return View(request);
            }

            return RedirectToRoute(
                "empresa",
                new
                {
                    slugEmpresa,
                    controller = "Auto",
                    action = "Dashboard"
                });

        }

        // ==========================================================
        // ACCESO DENEGADO
        // ==========================================================

        [HttpGet]
        public IActionResult AccesoDenegado(
            string? motivo)
        {
            ViewBag.Motivo =
                string.IsNullOrWhiteSpace(motivo)
                    ? "La empresa no está autorizada para utilizar el ERP."
                    : motivo;

            return View();
        }


        // ==========================================================
        // DASHBOARD
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard(
            string? slugEmpresa)
        {
                    // ======================================================
                    // VALIDAR SESIÓN DEL USUARIO
                    // ======================================================

                    if (string.IsNullOrWhiteSpace(
                        HttpContext.Session.GetString("UsuarioID")))
                    {
                        if (string.IsNullOrWhiteSpace(slugEmpresa))
                        {
                            slugEmpresa =
                                HttpContext.Session.GetString(
                                    "SlugEmpresa");
                        }

                        if (!string.IsNullOrWhiteSpace(slugEmpresa))
                        {
                            return RedirectToRoute(
                                "empresa",
                                new
                                {
                                    slugEmpresa,
                                    controller = "Auto",
                                    action = "Login"
                                });
                        }

                        return RedirectToAction(
                            nameof(Login));
                    }

                    // ======================================================
                    // RECUPERAR SLUG DE LA EMPRESA
                    // ======================================================

                    if (string.IsNullOrWhiteSpace(slugEmpresa))
                    {
                        slugEmpresa =
                            HttpContext.Session.GetString(
                                "SlugEmpresa");
                    }

                    if (string.IsNullOrWhiteSpace(slugEmpresa))
                    {
                        return RedirectToAction(
                            nameof(Login));
                    }

                    // ======================================================
                    // VALIDAR TENANT RESUELTO
                    // ======================================================

                    if (!_tenantContext.EstaResuelto ||
                        !_tenantContext.IdEmpresa.HasValue)
                    {
                        return RedirectToRoute(
                            "empresa",
                            new
                            {
                                slugEmpresa,
                                controller = "Auto",
                                action = "Login"
                            });
                    }

                    // ======================================================
                    // CREAR CONTEXTO DEL TENANT ACTUAL
                    // ======================================================

                    await using var tenantContext =
                        _tenantDbContextFactory.CreateDbContext();

                    // ======================================================
                    // FECHA ACTUAL
                    // ======================================================

                    var inicioDia =
                        DateTime.Today;

                    var finDia =
                        inicioDia.AddDays(1);

                    // ======================================================
                    // DESPACHOS REALIZADOS HOY
                    // ======================================================

                    var pedidosIds =
                        await tenantContext.Despachos
                            .Where(d =>
                                d.Estado ==
                                    EstadoDespacho.Despachado &&
                                d.Fecha >= inicioDia &&
                                d.Fecha < finDia)
                            .Select(d => d.ID_Pedido)
                            .ToListAsync();

                    var totalDespachos =
                        pedidosIds.Count;

                    // ======================================================
                    // VENTAS HOY
                    // ======================================================

                    var ventasHoy =
                        await tenantContext.Pedidos
                            .Where(p =>
                                pedidosIds.Contains(
                                    p.ID_Pedido))
                            .SumAsync(
                                p => (decimal?)p.TotalVenta) ?? 0m;

                    // ======================================================
                    // STOCK TOTAL
                    // ======================================================

                    var stockTotal =
                        await tenantContext.Productos
                            .Where(p => p.Activo)
                            .SumAsync(
                                p => (int?)p.Stock) ?? 0;

                    // ======================================================
                    // STOCK BAJO
                    // ======================================================

                    var stockBajo =
                        await tenantContext.Productos
                            .Where(p =>
                                p.Activo &&
                                p.Stock <= 10)
                            .CountAsync();

                    // ======================================================
                    // PRODUCCIÓN ACTIVA
                    // ======================================================

                    var produccionActiva =
                        await tenantContext.Producciones
                            .Where(p => p.Activo)
                            .CountAsync();

                    // ======================================================
                    // MODELO
                    // ======================================================

                    var model =
                        new DashboardViewModel
                        {
                            VentasHoy =
                                ventasHoy,

                            TotalDespachos =
                                totalDespachos,

                            StockTotal =
                                stockTotal,

                            StockBajo =
                                stockBajo,

                            ProduccionActiva =
                                produccionActiva
                        };

                    // ======================================================
                    // INFORMACIÓN PARA LA VISTA
                    // ======================================================

                    ViewBag.NombreUsuario =
                        HttpContext.Session.GetString(
                            "UsuarioNombre");

                    ViewBag.SlugEmpresa =
                        slugEmpresa;

                    ViewBag.IdEmpresa =
                        _tenantContext.IdEmpresa;

                    return View(model);
        }


        // ==========================================================
        // REGISTRO DE NUEVO USUARIO
        // ==========================================================
        [HttpGet]
        public IActionResult Register() => View();

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
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
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
            if (!ModelState.IsValid) return View(request);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == request.Correo);
            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo no registrado.");
                return View(request);
            }

            // Generar token (ejemplo simple)
            var token = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("ResetToken", token);
            HttpContext.Session.SetString("ResetUserId", usuario.IdUsuario.ToString());

            ViewBag.Mensaje = $"Token generado: {token} (en producción se enviaría por correo).";
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

            // Actualizar contraseña
            usuario.HashContrasena = BCrypt.Net.BCrypt.HashPassword(request.NuevaContrasena.Trim());
            usuario.FechaUltimaActualizacion = DateTime.Now;
            _context.SaveChanges();

            // Limpiar token de sesión
            HttpContext.Session.Remove("ResetToken");
            HttpContext.Session.Remove("ResetUserId");

            ViewBag.Mensaje = "Contraseña actualizada con éxito. Puede iniciar sesión.";
            return RedirectToAction("Login");
        }
    }

    // DTO adicional para recuperación de contraseña
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; } = string.Empty;
    }
}
