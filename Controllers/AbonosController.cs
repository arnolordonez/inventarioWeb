using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioWEB.Controllers
{
    public class AbonosController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        public AbonosController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 🔹 INDEX
        // ==========================================================
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================================
        // 🔹 BUSCAR CLIENTES
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> BuscarClientes(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            term = term.Trim().ToLower();

            var query = _context.Clientes
                .AsNoTracking()
                .Where(c => c.Activo);

            if (int.TryParse(term, out int idBusqueda))
            {
                query = query.Where(c => c.ID_Cliente == idBusqueda);
            }
            else
            {
                query = query.Where(c =>
                    (c.Nombre ?? "").Contains(term) ||
                    (c.Apellido ?? "").Contains(term) ||
                    ((c.Nombre ?? "") + " " + (c.Apellido ?? ""))
                        .Contains(term)
                );
            }

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .Select(c => new
                {
                    id_Cliente = c.ID_Cliente,
                    cedula = c.ID_Cliente,
                    nombreCompleto = ((c.Nombre ?? "") + " " + (c.Apellido ?? "")).Trim()
                })
                .Take(10)
                .ToListAsync();

            return Json(clientes);
        }

        // ==========================================================
        // 🔹 OBTENER PEDIDOS PENDIENTES
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerPedidosPendientes(int idCliente)
        {
            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .Where(p =>
                    p.ID_Cliente == idCliente &&
                    p.Saldo > 0 &&
                    p.EstadoPago != "PAGADO"
                )
                .OrderByDescending(p => p.Fecha)
                .Select(p => new
                {
                    id_Pedido = p.ID_Pedido,
                    fecha = p.Fecha.ToString("yyyy-MM-dd"),
                    totalVenta = p.TotalVenta,
                    saldo = p.Saldo,
                    tipoVenta = p.TipoVenta,
                    estadoPago = p.EstadoPago
                })
                .ToListAsync();

            return Json(pedidos);
        }

        // ==========================================================
        // 🔹 OBTENER MÉTODOS DE PAGO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerMetodosPago()
        {
            var data = await _context.MetodosPago
                .Where(m => m.Activo)
                .Select(m => new
                {
                    value = m.ID_MetodoPago,
                    text = m.Nombre
                })
                .OrderBy(m => m.text)
                .ToListAsync();

            return Json(data);
        }

        // ==========================================================
        // 🔹 REGISTRAR ABONO FINAL
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] AbonoVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ==================================================
                // 🔥 VALIDACIONES
                // ==================================================

                if (model == null)
                    return BadRequest("Datos inválidos.");

                if (model.ID_Pedido <= 0)
                    return BadRequest("Pedido inválido.");

                if (model.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero.");

                // ==================================================
                // 🔍 PEDIDO
                // ==================================================

                var pedido = await _context.Pedidos
                    .FirstOrDefaultAsync(p => p.ID_Pedido == model.ID_Pedido);

                if (pedido == null)
                    return BadRequest("Pedido no encontrado.");

                if (pedido.EstadoPago == "PAGADO")
                    return BadRequest("El pedido ya está pagado.");

                if (pedido.Saldo <= 0)
                    return BadRequest("El pedido no tiene saldo pendiente.");

                // ==================================================
                // 🔥 SOLO PAGO TOTAL
                // ==================================================

                if (model.Monto != pedido.Saldo)
                {
                    return BadRequest(
                        $"Debe pagar el saldo exacto: {pedido.Saldo:N2}"
                    );
                }

                // ==================================================
                // 🔥 CREAR ABONO
                // ==================================================

                var abono = new Abono
                {
                    ID_Pedido = pedido.ID_Pedido,
                    Fecha_Abono = DateTime.Now,
                    Monto = model.Monto,
                    ID_MetodoPago = model.ID_MetodoPago
                };

                _context.Abonos.Add(abono);

                // ==================================================
                // 🔥 ACTUALIZAR PEDIDO
                // ==================================================

                pedido.Saldo = 0;
                pedido.EstadoPago = "PAGADO";

                // ==================================================
                // 🔥 GUARDAR
                // ==================================================

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    mensaje = "Abono registrado correctamente.",
                    pedido = pedido.ID_Pedido,
                    estado = pedido.EstadoPago,
                    saldo = pedido.Saldo
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return BadRequest(
                    ex.InnerException?.Message ?? ex.Message
                );
            }
        }

        // ==========================================================
        // 🔹 DETALLE DEL PEDIDO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerDetallePedido(int idPedido)
        {
            var detalles = await _context.DetallePedidos
                .AsNoTracking()
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Referencia)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Talla)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.ColorNav)
                .Where(d => d.ID_Pedido == idPedido)
                .Select(d => new
                {
                    producto =
                        (d.Producto.Referencia != null
                            ? d.Producto.Referencia.DescripReferencia
                            : "N/A"),

                    talla =
                        (d.Producto.Talla != null
                            ? d.Producto.Talla.DescripTalla
                            : "N/A"),

                    color =
                        !string.IsNullOrEmpty(d.Producto.ColorSnapshot)
                            ? d.Producto.ColorSnapshot
                            : (d.Producto.ColorNav != null
                                ? d.Producto.ColorNav.Nombre
                                : "N/A"),

                    cantidad = d.Cantidad,
                    subtotal = d.Subtotal
                })
                .ToListAsync();

            return Json(detalles);
        }
    }
}
