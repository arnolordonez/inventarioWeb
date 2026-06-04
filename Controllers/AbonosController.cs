using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.Services;
using InventarioWEB.ViewModels;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioWEB.Controllers
{
    public class AbonosController : Controller
    {
        private readonly MovimientoVentasDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly AbonoService _abonoService;

        private readonly ReciboCajaService _reciboCajaService;

        public AbonosController(
            MovimientoVentasDbContext context,
            IWebHostEnvironment env,
            AbonoService abonoService,
            ReciboCajaService reciboCajaService)
        {
            _context = context;
            _env = env;
            _abonoService = abonoService;
            _reciboCajaService = reciboCajaService;
        }

        // ==========================================================
        // 🔹 INDEX
        // ==========================================================
        public IActionResult Index()
        {
            return View();
        }


        /*using InventarioWEB.Data;
        using InventarioWEB.Models;
        using InventarioWEB.ViewModels;
        using iText.IO.Font.Constants;
        using iText.Kernel.Font;
        using iText.Layout.Element;
        using iText.Layout.Properties;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.EntityFrameworkCore;

        namespace InventarioWEB.Controllers
        {
            public class AbonosController : Controller
            {
                private readonly MovimientoVentasDbContext _context;
                private readonly IWebHostEnvironment _env;

                public AbonosController(MovimientoVentasDbContext context, IWebHostEnvironment env)
                {
                    _context = context;
                    _env = env;
                }

                // ==========================================================
                // 🔹 INDEX
                // ==========================================================
                public IActionResult Index()
                {
                    return View();
                }
                */
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



        /*

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
         */


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
                // 🔥 OBTENER USUARIO LOGUEADO
                // ==================================================

                var usuarioIdStr =
                    HttpContext.Session.GetString("UsuarioID");

                if (string.IsNullOrEmpty(usuarioIdStr))
                    return BadRequest("Sesión de usuario no válida.");

                int usuarioId = int.Parse(usuarioIdStr);

                var usuarioNombre =
                 HttpContext.Session.GetString("UsuarioNombre");

                // ==================================================
                // 🔥 CREAR ABONO
                // ==================================================

                var abono = new Abono
                {
                    ID_Pedido = pedido.ID_Pedido,
                    Fecha_Abono = DateTime.Now,
                    Monto = model.Monto,
                    ID_MetodoPago = model.ID_MetodoPago,
                    ID_Usuario = usuarioId,
                    UsuarioRegistro = usuarioNombre, // NUEVO
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
                // 🔥 GENERAR NÚMERO DE RECIBO
                // ==================================================

                abono.NumeroRecibo =
                    $"RC-{DateTime.Now:yyyy-MM}-{abono.ID_Abono:D6}";

                abono.RutaRecibo =
                    $"ReciboCaja/{abono.NumeroRecibo}.pdf";

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

                if (pedido.Saldo < 0)
                    pedido.Saldo = 0;

                // ==================================================
                // 🔥 ACTUALIZAR ESTADO PAGO
                // ==================================================

                pedido.EstadoPago =
                    pedido.Saldo <= 0
                        ? "PAGADO"
                        : "ABONADO";

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // ==================================================
                // 🔥 GENERAR RECIBO DE CAJA PDF
                // ==================================================

                _reciboCajaService.GenerarPDF(abono.ID_Abono);

                return Ok(new
                {
                    success = true,
                    mensaje = "Abono registrado correctamente.",

                    idAbono = abono.ID_Abono,

                    urlPdf =
                        $"/Abonos/GenerarReciboCajaPDF?idAbono={abono.ID_Abono}",

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
        // 🔹 RESUMEN FINANCIERO PEDIDO
        // ==========================================================
        [HttpGet]
        // ==========================================================
        // 🔹 RESUMEN FINANCIERO PEDIDO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerResumenPedido(int idPedido)
        {
            var pedido = await _context.Pedidos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID_Pedido == idPedido);

            if (pedido == null)
                return NotFound();

            var totalAbonado = await _context.Abonos
                .Where(a =>
                    a.ID_Pedido == idPedido &&
                    a.Activo)
                .SumAsync(a => (decimal?)a.Monto) ?? 0;
                       
            var saldoCalculado =
               pedido.TotalVenta - totalAbonado;

            if (saldoCalculado < 0)
               saldoCalculado = 0;

            return Json(new
            {
                totalVenta = pedido.TotalVenta,
                totalAbonado = totalAbonado,
                saldo = saldoCalculado,
                estadoPago = pedido.EstadoPago
            });
                        
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHistorialAbonos(int idPedido)
        {
            var historial = await _context.Abonos
                .AsNoTracking()
                .Include(a => a.MetodoPago)
                .Where(a =>
                    a.ID_Pedido == idPedido &&
                    a.Activo)
                .OrderByDescending(a => a.ID_Abono)
                .Select(a => new
                {
                    idAbono = a.ID_Abono,

                    numeroRecibo =
                        a.NumeroRecibo,

                    fecha =
                        a.Fecha_Abono.ToString("yyyy-MM-dd"),

                    monto =
                        a.Monto,

                    metodoPago =
                        a.MetodoPago != null
                            ? a.MetodoPago.Nombre
                            : "",

                    usuario =
                        a.UsuarioRegistro ?? "",

                    observacion =
                        a.Observacion ?? "",

                    urlPdf =
                        $"/Abonos/GenerarReciboCajaPDF?idAbono={a.ID_Abono}"
                })
                .ToListAsync();

            return Json(historial);
        }

        // ======================================================
        // 🔥 VER / DESCARGAR RECIBO DE CAJA GENERADO
        // ======================================================

        [HttpGet]
        public IActionResult GenerarReciboCajaPDF(int idAbono)
        {
            var abono = _context.Abonos
                .FirstOrDefault(a => a.ID_Abono == idAbono);

            if (abono == null)
                return NotFound("Abono no encontrado");

            // ======================================================
            // 🔥 VALIDAR RUTA DEL PDF
            // ======================================================

            if (string.IsNullOrWhiteSpace(abono.RutaRecibo))
                return NotFound("El recibo no tiene una ruta asociada.");

            // ======================================================
            // 🔥 OBTENER RUTA FÍSICA
            // ======================================================

            var rutaFisica = Path.Combine(
                _env.WebRootPath,
                abono.RutaRecibo.Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            
            // 🔥 VALIDAR EXISTENCIA DEL ARCHIVO
            // ======================================================

            if (!System.IO.File.Exists(rutaFisica))
            {
                // Intentar regenerar el PDF

                _reciboCajaService.GenerarPDF(idAbono);

                if (!System.IO.File.Exists(rutaFisica))
                {
                    return NotFound(
                        $"No fue posible generar el PDF. Ruta esperada: {rutaFisica}"
                    );
                }
            }

            // if (!System.IO.File.Exists(rutaFisica))
            //   return NotFound("El archivo PDF no existe.");

            // ======================================================
            // 🔥 MOSTRAR PDF
            // ======================================================

            return PhysicalFile(
                rutaFisica,
                "application/pdf",
                Path.GetFileName(rutaFisica)
            );
        }


    }
}