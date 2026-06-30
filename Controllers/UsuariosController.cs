using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using System.Security.Cryptography;
using BCrypt.Net;
using InventarioWEB.Filters;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Administración de usuarios del sistema ERP.
    /// Acceso exclusivo para Administradores.
    /// </summary>
    [ValidarSesion]
    public class UsuariosController : Controller
    {
        // ==========================================================
        // CONTEXTO
        // ==========================================================

        private readonly UsuariosDbContext _context;

        // ==========================================================
        // CONSTRUCTOR
        // ==========================================================

        public UsuariosController(UsuariosDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // VALIDAR ADMINISTRADOR
        // ==========================================================

        private bool EsAdministrador()
        {
            return HttpContext.Session.GetString("Rol") == "Administrador";
        }
        // ==========================================================
        // VALIDAR ÚLTIMO ADMINISTRADOR ACTIVO
        // ==========================================================

        private async Task<bool> EsUltimoAdministradorActivo(int idUsuario)
        {
            int cantidadAdministradores =
                await _context.Usuarios
                    .CountAsync(u =>
                        u.Activo &&
                        u.IdRol == 1);

            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.IdUsuario == idUsuario);

            return usuario != null
                   && usuario.Activo
                   && usuario.IdRol == 1
                   && cantidadAdministradores == 1;
        }
        // ==========================================================
        // LISTADO DE USUARIOS
        // ==========================================================

        public async Task<IActionResult> Index()
        {
            if (!EsAdministrador())
                return RedirectToAction(
                    "AccesoDenegado",
                    "Auto");

            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .AsNoTracking()
                .ToListAsync();

            return View(usuarios);
        }

        // ==========================================================
        // CREAR USUARIO (GET)
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!EsAdministrador())
                return RedirectToAction(
                    "AccesoDenegado",
                    "Auto");

            ViewBag.Roles = await _context.Roles
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            return View();
        }
        // ==========================================================
        // CREAR USUARIO (POST)
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateViewModel model)
        {
            if (!EsAdministrador())
                return RedirectToAction(
                    "AccesoDenegado",
                    "Auto");

            // ==========================================
            // VALIDACIONES DEL MODELO
            // ==========================================

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles
                    .OrderBy(r => r.NombreRol)
                    .ToListAsync();

                return View(model);
            }

            // ==========================================
            // VALIDAR CONTRASEÑAS
            // ==========================================

            if (model.Contrasena != model.ConfirmarContrasena)
            {
                ModelState.AddModelError(
                    "ConfirmarContrasena",
                    "Las contraseñas no coinciden.");

                ViewBag.Roles = await _context.Roles
                    .OrderBy(r => r.NombreRol)
                    .ToListAsync();

                return View(model);
            }

            // ==========================================
            // VALIDAR CORREO DUPLICADO
            // ==========================================

            bool existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.Correo == model.Correo);

            if (existeCorreo)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Ya existe un usuario registrado con este correo.");

                ViewBag.Roles = await _context.Roles
                    .OrderBy(r => r.NombreRol)
                    .ToListAsync();

                return View(model);
            }

            // ==========================================
            // GENERAR SALT
            // ==========================================

            string salt = Guid.NewGuid().ToString();

            // ==========================================
            // GENERAR HASH BCRYPT
            // ==========================================

            string hashPassword =
                BCrypt.Net.BCrypt.HashPassword(
                    model.Contrasena.Trim());

            // ==========================================
            // CREAR ENTIDAD
            // ==========================================

            var usuario = new Usuario
            {
                Nombres = model.Nombres.Trim(),
                Apellidos = model.Apellidos.Trim(),
                Correo = model.Correo.Trim().ToLower(),

                HashContrasena = hashPassword,
                Salt = salt,

                IdRol = model.IdRol,
                Activo = model.Activo,

                FechaCreacion = DateTime.Now,
                FechaUltimaActualizacion = DateTime.Now
            };

            // ==========================================
            // GUARDAR
            // ==========================================

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            TempData["success"] =
                $"Usuario {usuario.Nombres} {usuario.Apellidos} creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================================
        // DESACTIVAR USUARIO
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Desactivar(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction(
                    "AccesoDenegado",
                    "Auto");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // ==========================================
            // NO PERMITIR DESACTIVARSE A SÍ MISMO
            // ==========================================

            var usuarioActualId =
                HttpContext.Session.GetString("UsuarioID");

            if (usuarioActualId == usuario.IdUsuario.ToString())
            {
                TempData["error"] =
                    "No puede desactivar su propio usuario.";

                return RedirectToAction(nameof(Index));
            }

            // ==========================================
            // NO PERMITIR DESACTIVAR
            // EL ÚLTIMO ADMINISTRADOR
            // ==========================================

            if (await EsUltimoAdministradorActivo(id))
            {
                TempData["error"] =
                    "No es posible desactivar el último Administrador activo del sistema.";

                return RedirectToAction(nameof(Index));
            }

            usuario.Activo = false;
            usuario.FechaUltimaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["success"] =
                $"Usuario {usuario.Nombres} {usuario.Apellidos} desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================================
        // ACTIVAR USUARIO
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Activar(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction(
                    "AccesoDenegado",
                    "Auto");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            usuario.Activo = true;
            usuario.FechaUltimaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["success"] =
                $"Usuario {usuario.Nombres} {usuario.Apellidos} activado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        // ==========================================================
        // EDITAR USUARIO (GET)
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!EsAdministrador())
                return RedirectToAction(
                    "AccesoDenegado",
                    "Auto");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.Roles
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            var model = new UsuarioEditViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Correo = usuario.Correo,
                IdRol = usuario.IdRol,
                Activo = usuario.Activo
            };

            return View(model);
        }

        // ==========================================================
        // EDITAR USUARIO (POST)
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioEditViewModel model)
        {
            if (!EsAdministrador())
                return RedirectToAction(
                    "AccesoDenegado",
                    "Auto");

            // ==========================================
            // VALIDACIÓN DEL MODELO
            // ==========================================

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles
                    .OrderBy(r => r.NombreRol)
                    .ToListAsync();

                return View(model);
            }

            // ==========================================
            // BUSCAR USUARIO
            // ==========================================

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == model.IdUsuario);

            if (usuario == null)
            {
                TempData["error"] =
                    "Usuario no encontrado.";

                return RedirectToAction(nameof(Index));
            }

            // ==========================================
            // VALIDAR CORREO DUPLICADO
            // ==========================================

            bool correoDuplicado = await _context.Usuarios
                .AnyAsync(u =>
                    u.Correo == model.Correo &&
                    u.IdUsuario != model.IdUsuario);

            if (correoDuplicado)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Ya existe otro usuario con este correo.");

                ViewBag.Roles = await _context.Roles
                    .OrderBy(r => r.NombreRol)
                    .ToListAsync();

                return View(model);
            }

            // ==========================================
            // NO PERMITIR MODIFICAR
            // SU PROPIO ROL
            // ==========================================

            var usuarioActualId =
                HttpContext.Session.GetString("UsuarioID");

            if (usuarioActualId ==
                usuario.IdUsuario.ToString())
            {
                bool cambiaRol =
                    usuario.IdRol != model.IdRol;

                bool seDesactiva =
                    !model.Activo;

                if (cambiaRol || seDesactiva)
                {
                    ModelState.AddModelError(
                        "",
                        "No puede cambiar su propio rol ni desactivar su cuenta.");

                    ViewBag.Roles = await _context.Roles
                        .OrderBy(r => r.NombreRol)
                        .ToListAsync();

                    return View(model);
                }
            }

            // ==========================================
            // PROTEGER ÚLTIMO ADMINISTRADOR
            // ==========================================

            bool esAdministrador =
                usuario.IdRol == 1;

            bool cambiaraRol =
                model.IdRol != 1;

            bool seraDesactivado =
                !model.Activo;

            if (esAdministrador &&
                (cambiaraRol || seraDesactivado))
            {
                if (await EsUltimoAdministradorActivo(
                    usuario.IdUsuario))
                {
                    ModelState.AddModelError(
                        "",
                        "No es posible modificar el último Administrador activo del sistema.");

                    ViewBag.Roles = await _context.Roles
                        .OrderBy(r => r.NombreRol)
                        .ToListAsync();

                    return View(model);
                }
            }
            // ==========================================
            // ACTUALIZAR DATOS
            // ==========================================

            usuario.Nombres = model.Nombres.Trim();
            usuario.Apellidos = model.Apellidos.Trim();
            usuario.Correo = model.Correo.Trim().ToLower();
            usuario.IdRol = model.IdRol;
            usuario.Activo = model.Activo;

            // ==========================================
            // CAMBIO DE CONTRASEÑA (OPCIONAL)
            // ==========================================

            if (!string.IsNullOrWhiteSpace(
                model.NuevaContrasena))
            {
                usuario.HashContrasena =
                    BCrypt.Net.BCrypt.HashPassword(
                        model.NuevaContrasena.Trim());

                usuario.Salt = Guid.NewGuid()
                    .ToString();
            }

            usuario.FechaUltimaActualizacion =
                DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["success"] =
                $"Usuario {usuario.Nombres} {usuario.Apellidos} actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

    }
}