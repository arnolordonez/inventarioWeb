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

            // ======================================================
            // 🔥 BÚSQUEDA POR CÉDULA
            // ======================================================
            if (int.TryParse(term, out int idBusqueda))
            {
                query = query.Where(c => c.ID_Cliente == idBusqueda);
            }
            else
            {
                // ==================================================
                // 🔥 BÚSQUEDA POR NOMBRE
                // ==================================================
                query = query.Where(c =>
                    (c.Nombre ?? "").ToLower().Contains(term) ||
                    (c.Apellido ?? "").ToLower().Contains(term) ||
                    (
                        ((c.Nombre ?? "") + " " + (c.Apellido ?? ""))
                        .ToLower()
                    ).Contains(term)
                );
            }

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .Select(c => new
                {
                    id_Cliente = c.ID_Cliente,
                    cedula = c.ID_Cliente,
                    nombreCompleto =
                        ((c.Nombre ?? "") + " " + (c.Apellido ?? "")).Trim()
                })
                .Take(10)
                .ToListAsync();

            return Json(clientes);
        }

        // ==========================================================
        // 🔹 OBTENER PEDIDOS CON SALDO
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
                    estadoPago = p.EstadoPago,
                    estado = p.Estado
                })
                .ToListAsync();

            return Json(pedidos);
        }

        // ==========================================================
        // 🔹 MÉTODOS DE PAGO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerMetodosPago()
        {
            var data = await _context.MetodosPago
                .AsNoTracking()
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    value = m.ID_MetodoPago,
                    text = m.Nombre
                })
                .ToListAsync();

            return Json(data);
        }

        // ==========================================================
        // 🔹 REGISTRAR ABONO
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> RegistrarAbono([FromBody] AbonoVM model)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ==================================================
                // 🔥 VALIDAR MODELO
                // ==================================================

                if (model == null)
                    return BadRequest("Datos inválidos.");

                if (model.ID_Pedido <= 0)
                    return BadRequest("Pedido inválido.");

                if (model.Monto <= 0)
                    return BadRequest("El monto debe ser mayor a cero.");

                if (model.ID_MetodoPago <= 0)
                    return BadRequest("Debe seleccionar método de pago.");

                // ==================================================
                // 🔥 BUSCAR PEDIDO
                // ==================================================

                var pedido = await _context.Pedidos
                    .FirstOrDefaultAsync(p =>
                        p.ID_Pedido == model.ID_Pedido
                    );

                if (pedido == null)
                    return BadRequest("Pedido no encontrado.");

                // ==================================================
                // 🔥 VALIDAR ESTADO PEDIDO
                // ==================================================

                if (
                    pedido.Estado == "ANULADO" ||
                    pedido.Estado == "CANCELADO"
                )
                {
                    return BadRequest(
                        $"No se pueden registrar abonos para pedidos en estado: {pedido.Estado}"
                    );
                }

                // ==================================================
                // 🔥 VALIDAR ESTADO PAGO
                // ==================================================

                if (pedido.EstadoPago == "PAGADO")
                {
                    return BadRequest(
                        "El pedido ya se encuentra totalmente pagado."
                    );
                }

                // ==================================================
                // 🔥 VALIDAR SALDO
                // ==================================================

                if (pedido.Saldo <= 0)
                {
                    return BadRequest(
                        "El pedido no tiene saldo pendiente."
                    );
                }

                // ==================================================
                // 🔥 VALIDAR SOBREPAGO
                // ==================================================

                if (model.Monto > pedido.Saldo)
                {
                    return BadRequest(
                        $"El monto supera el saldo pendiente: {pedido.Saldo:N2}"
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
                    ID_MetodoPago = model.ID_MetodoPago,

                    // ==============================================
                    // 🔥 AUDITORÍA
                    // ==============================================
                    ID_Usuario = model.ID_Usuario,
                    Observacion = model.Observacion ?? "",
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                _context.Abonos.Add(abono);

                // ==================================================
                // 🔥 GUARDAR ABONO
                // ==================================================

                await _context.SaveChangesAsync();

                // ==================================================
                // 🔥 RECALCULAR TOTAL ABONADO
                // ==================================================

                var totalAbonado = await _context.Abonos
                    .Where(a =>
                        a.ID_Pedido == pedido.ID_Pedido &&
                        a.Activo
                    )
                    .SumAsync(a => (decimal?)a.Monto) ?? 0;

                // ==================================================
                // 🔥 RECALCULAR SALDO REAL
                // ==================================================

                pedido.Saldo = pedido.TotalVenta - totalAbonado;

                // ==================================================
                // 🔥 EVITAR DECIMALES NEGATIVOS
                // ==================================================

                if (pedido.Saldo < 0)
                    pedido.Saldo = 0;

                // ==================================================
                // 🔥 ACTUALIZAR ESTADO FINANCIERO
                // ==================================================

                pedido.EstadoPago =
                    pedido.Saldo <= 0
                        ? "PAGADO"
                        : "ABONADO";

                // ==================================================
                // 🔥 GUARDAR CAMBIOS
                // ==================================================

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // ==================================================
                // 🔥 RESPUESTA
                // ==================================================

                return Ok(new
                {
                    success = true,
                    mensaje = "Abono registrado correctamente.",
                    pedido = pedido.ID_Pedido,
                    estadoPago = pedido.EstadoPago,
                    saldo = pedido.Saldo,
                    totalAbonado = totalAbonado
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
        // 🔹 DETALLE PEDIDO
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
                        d.Producto.Referencia != null
                            ? d.Producto.Referencia.DescripReferencia
                            : "N/A",

                    talla =
                        d.Producto.Talla != null
                            ? d.Producto.Talla.DescripTalla
                            : "N/A",

                    color =
                        !string.IsNullOrEmpty(
                            d.Producto.ColorSnapshot
                        )
                            ? d.Producto.ColorSnapshot
                            : (
                                d.Producto.ColorNav != null
                                    ? d.Producto.ColorNav.Nombre
                                    : "N/A"
                            ),

                    cantidad = d.Cantidad,
                    subtotal = d.Subtotal
                })
                .ToListAsync();

            return Json(detalles);
        }

        // ==========================================================
        // 🔹 HISTORIAL ABONOS POR PEDIDO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerHistorialAbonos(int idPedido)
        {
            var abonos = await _context.Abonos
                .AsNoTracking()
                .Include(a => a.MetodoPago)
                .Include(a => a.Usuario)
                .Where(a =>
                    a.ID_Pedido == idPedido &&
                    a.Activo
                )
                .OrderByDescending(a => a.Fecha_Abono)
                .Select(a => new
                {
                    id_Abono = a.ID_Abono,
                    fecha = a.Fecha_Abono.ToString("yyyy-MM-dd HH:mm"),
                    monto = a.Monto,

                    metodoPago =
                        a.MetodoPago != null
                            ? a.MetodoPago.Nombre
                            : "N/A",

                    usuario =
                        a.Usuario != null
                            ? (
                                (a.Usuario.Nombres ?? "") + " " +
                                (a.Usuario.Apellidos ?? "")
                            ).Trim()
                            : "Sistema",

                    observacion = a.Observacion ?? ""
                })
                .ToListAsync();

            return Json(abonos);
        }
    }
}