using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.Models.Clientes.Dto;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.Controllers.Api
{
    [Route("api/clientes")]
    [ApiController]
    public class ClientesssApiController : ControllerBase
    {
        private readonly MovimientoVentasDbContext _context;

        public ClientesssApiController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // GET: api/clientes → lista de clientes activos
        // ==========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteListadoDto>>> GetClientes()
        {
            var clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new ClienteListadoDto
                {
                    ID_Cliente = c.ID_Cliente,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    Correo = c.Correo,
                    Telefono = c.Telefono,
                    TipoCliente = c.TipoCliente,
                    VIP = c.VIP
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // ==========================================================
        // GET: api/clientes/5 → detalle de un cliente
        // ==========================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDetalleDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Where(c => c.ID_Cliente == id && c.Activo)
                .Select(c => new ClienteDetalleDto
                {
                    ID_Cliente = c.ID_Cliente,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    Telefono = c.Telefono,
                    Correo = c.Correo,
                    Direccion = c.Direccion,
                    CiudadMunicipio = c.CiudadMunicipio,
                    TipoCliente = c.TipoCliente,
                    Observaciones = c.Observaciones,
                    FechaRegistro = c.FechaRegistro,
                    VIP = c.VIP,
                    Activo = c.Activo
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado" });

            return Ok(cliente);
        }

        // ==========================================================
        // POST: api/clientes → crear cliente
        // ==========================================================
        [HttpPost]
        public async Task<ActionResult> CrearCliente([FromBody] ClienteCrearDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar cédula única
            if (await _context.Clientes.AnyAsync(c => c.ID_Cliente == dto.ID_Cliente))
                return BadRequest(new { mensaje = "Ya existe un cliente con esta cédula." });

            // Validar correo
            if (!new EmailAddressAttribute().IsValid(dto.Correo))
                return BadRequest(new { mensaje = "Correo electrónico inválido." });

            // Crear salt
            string salt = Guid.NewGuid().ToString();

            var cliente = new Cliente
            {
                ID_Cliente = dto.ID_Cliente,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Telefono = dto.Telefono,
                Correo = dto.Correo,
                Direccion = dto.Direccion,
                CiudadMunicipio = dto.CiudadMunicipio,
                TipoCliente = dto.TipoCliente,
                Observaciones = dto.Observaciones,
                FechaRegistro = DateTime.Now,
                Activo = true,
                VIP = false,

                Salt = salt,
                HashContrasena = BCrypt.Net.BCrypt.HashPassword("default123" + salt)
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cliente creado correctamente" });
        }

        // ==========================================================
        // PUT: api/clientes/5 → actualizar
        // ==========================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarCliente(int id, [FromBody] ClienteActualizarDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null || !cliente.Activo)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            cliente.Nombre = dto.Nombre;
            cliente.Apellido = dto.Apellido;
            cliente.Telefono = dto.Telefono;
            cliente.Correo = dto.Correo;
            cliente.Direccion = dto.Direccion;
            cliente.CiudadMunicipio = dto.CiudadMunicipio;
            cliente.TipoCliente = dto.TipoCliente;
            cliente.Observaciones = dto.Observaciones;
            cliente.VIP = dto.VIP;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cliente actualizado correctamente" });
        }

        // ==========================================================
        // DELETE: api/clientes/5 → inactivación lógica
        // ==========================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null || !cliente.Activo)
                return NotFound(new { mensaje = "Cliente no encontrado" });

            cliente.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cliente desactivado" });
        }
    }
}
