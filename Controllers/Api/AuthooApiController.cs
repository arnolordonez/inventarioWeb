using InventarioWEB.Models.API;
using InventarioWEB.Models;
using InventarioWEB.Data;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Linq;

namespace InventarioWEB.Controllers.Api
{
    /// <summary>
    /// API REST encargada de gestionar la autenticación
    /// y el registro de usuarios del sistema InventarioWEB.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthooApiController : ControllerBase
    {
        private readonly UsuariosDbContext _context;

        /// <summary>
        /// Constructor que recibe el contexto de base de datos
        /// mediante inyección de dependencias.
        /// </summary>
        public AuthooApiController(UsuariosDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Endpoint de prueba para verificar que la API está funcionando.
        /// </summary>
        [HttpGet("login-test")]
        public IActionResult LoginTest()
        {
            return Ok("Endpoint login funcionando");
        }

        // ==========================================================
        // SERVICIO WEB: INICIO DE SESIÓN
        // ==========================================================

        /// <summary>
        /// Servicio web que valida las credenciales de un usuario.
        /// Recibe correo y contraseña, valida contra la base de datos
        /// y devuelve si la autenticación fue exitosa o no.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validar datos recibidos
            if (request == null || string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Mensaje = "Datos de autenticación incompletos",
                    Autenticado = false
                });
            }

            // Buscar usuario por correo
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == request.Correo);

            // Validar existencia y estado
            if (usuario == null || usuario.Activo == false)
            {
                return Unauthorized(new AuthResponse
                {
                    Mensaje = "Error en la autenticación",
                    Autenticado = false
                });
            }

            // Verificar contraseña usando BCrypt
            bool passwordValida = BCrypt.Net.BCrypt.Verify(
                request.Password,
                usuario.HashContrasena
            );

            if (!passwordValida)
            {
                return Unauthorized(new AuthResponse
                {
                    Mensaje = "Error en la autenticación",
                    Autenticado = false
                });
            }

            // Autenticación exitosa
            return Ok(new AuthResponse
            {
                Mensaje = "Autenticación satisfactoria",
                Autenticado = true
            });
        }

        // ==========================================================
        // SERVICIO WEB: REGISTRO DE USUARIOS
        // ==========================================================

        /// <summary>
        /// Servicio web que permite registrar un nuevo usuario
        /// en el sistema.
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Validar datos recibidos
            if (request == null ||
                string.IsNullOrEmpty(request.Correo) ||
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Mensaje = "Datos de registro incompletos",
                    Autenticado = false
                });
            }

            // Verificar si el correo ya existe
            var existeUsuario = _context.Usuarios
                .Any(u => u.Correo == request.Correo);

            if (existeUsuario)
            {
                return Conflict(new AuthResponse
                {
                    Mensaje = "El correo ya está registrado",
                    Autenticado = false
                });
            }

            // Generar hash seguro de la contraseña
            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Crear objeto usuario
            var usuario = new Usuario
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Correo = request.Correo,
                HashContrasena = hash,
                IdRol = request.IdRol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            // Guardar en base de datos
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return Ok(new AuthResponse
            {
                Mensaje = "Usuario registrado correctamente",
                Autenticado = true
            });
        }
    }
}