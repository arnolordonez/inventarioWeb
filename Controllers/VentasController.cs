using InventarioWEB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using Microsoft.AspNetCore.Hosting;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using InventarioWEB.Services;
using iText.Commons.Actions.Contexts;
using InventarioWEB.Filters;

using System.Diagnostics;  // Usada para diagnostico de rendimiento
namespace InventarioWEB.Controllers
{
    [ValidarSesion]
    public class VentasController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        private readonly IWebHostEnvironment _env;

        private readonly ReciboCajaService _reciboCajaService;

        public VentasController(MovimientoVentasDbContext context, IWebHostEnvironment env, ReciboCajaService reciboCajaService)
        {
            _context = context;
            _env = env;
            _reciboCajaService = reciboCajaService;
        }

        private bool TieneAcceso()
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;
            //var rol = HttpContext.Session.GetString("Rol");

            return rol == "Administrador"
                || rol == "Vendedor";
        }

        // ==========================================================
        // 🔹 INDEX (POS)
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            await CargarFiltrosBaseAsync();
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

            // 🔥 Búsqueda inteligente (RÁPIDA)
            if (int.TryParse(term, out int idBusqueda))
            {
                query = query.Where(c => c.ID_Cliente == idBusqueda);
            }
            else
            {
                term = term.Trim();

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
                    nombreCompleto = ((c.Nombre ?? "") + " " + (c.Apellido ?? "")).Trim(),
                    ClienteId = c.ID_Cliente
                })
                .Take(10)
                .ToListAsync();

            return Json(clientes.Select(c => new
            {
                c.id_Cliente,
                c.cedula,
                c.nombreCompleto,
                tieneDeuda = false,
                totalDeuda = 0,
                estado = "OK"
            }));

        }

        [HttpGet]
        public async Task<IActionResult> ObtenerReferenciasPorGenero(int idGenero)
        {
            var data = await _context.Referencias
                .Where(r => r.ID_Genero == idGenero && r.Activo)
                .Select(r => new
                {
                    value = r.ID_Referencias,
                    text = r.DescripReferencia
                })
                .OrderBy(r => r.text)
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerGeneros()
        {
            var data = await _context.Generos
                .Select(g => new
                {
                    value = g.ID_Genero,
                    text = g.DescripGenero
                })
                .OrderBy(g => g.text)
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTelas()
        {
            var data = await _context.Telas
                .Where(t => t.Activo)
                .Select(t => new
                {
                    value = t.ID_Telas,
                    text = t.DescripTela
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerColores()
        {
            var data = await _context.Colores
                .Where(c => c.Activo)
                .Select(c => new
                {
                    value = c.ID_Color,
                    text = c.Nombre
                })
                .ToListAsync();

            return Json(data);
        }

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

        
        [HttpGet]
        public async Task<IActionResult> ObtenerMatriz(int idGenero, int idReferencia, int idTela, int idColor)
        {
            var productos = await _context.Productos
                .Include(p => p.Talla)
                .Where(p =>
                    p.ID_Genero == idGenero &&
                    p.ID_Referencias == idReferencia &&
                    p.ID_Telas == idTela &&
                    p.ID_Color == idColor &&
                    p.Activo
                )
                .Select(p => new
                {
                    id_Producto = p.ID_Producto,
                    talla = p.Talla != null ? p.Talla.DescripTalla : "Sin talla",
                    stock = p.Stock,
                    precioVTA = p.PrecioVTA
                })
                .OrderBy(p => p.talla)
                .ToListAsync();

            return Json(productos);
        }
        

        /*
        // METODO UTILIZADO PARA MEDIR RENDIMIENTO DE LA CONSULTA DE MATRIZ DE PRODUCTOS (PNF-002)
        [HttpGet]
    public async Task<IActionResult> ObtenerMatriz(int idGenero, int idReferencia, int idTela, int idColor)
    {
        // INICIO MEDICIÓN PNF-002
        var sw = Stopwatch.StartNew();
        var productos = await _context.Productos
            .Include(p => p.Talla)
            .Where(p =>
                p.ID_Genero == idGenero &&
                p.ID_Referencias == idReferencia &&
                p.ID_Telas == idTela &&
                p.ID_Color == idColor &&
                p.Activo
            )
            .Select(p => new
            {
                id_Producto = p.ID_Producto,
                talla = p.Talla != null ? p.Talla.DescripTalla : "Sin talla",
                stock = p.Stock,
                precioVTA = p.PrecioVTA
            })
            .OrderBy(p => p.talla)
            .ToListAsync();

        // FIN MEDICIÓN PNF-002
        sw.Stop();

        Debug.WriteLine(
            $"PNF-002 -> ObtenerMatriz ejecutado en {sw.ElapsedMilliseconds} ms");

        return Json(productos);
    }
    */


    // ==========================================================
    // 🔹 GUARDAR VENTA (VERSIÓN ESTABLE - CORREGIDA + URL PDF)
    // ==========================================================
    [HttpPost]
        public async Task<IActionResult> Crear([FromBody] VentaVM venta)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ======================================================
                // 🔥 VALIDACIONES BÁSICAS
                // ======================================================

                if (venta == null)
                    return BadRequest("Datos de venta inválidos.");

                if (venta.ID_Cliente <= 0)
                    return BadRequest("Cliente inválido.");

                if (venta.Detalles == null || !venta.Detalles.Any(x => x.Cantidad > 0))
                    return BadRequest("No hay productos válidos en la venta.");

                if (string.IsNullOrEmpty(venta.TipoVenta))
                    return BadRequest("El tipo de venta es obligatorio.");

                // ======================================================
                // 🔥 RECALCULAR TOTALES (BACKEND MANDA)
                // ======================================================
                decimal totalBase = 0;
                decimal totalIVA = 0;

                var detallesAgrupados = venta.Detalles
                    .Where(x => x.Cantidad > 0)
                    .GroupBy(x => x.ID_Producto)
                    .Select(g => new
                    {
                        ID_Producto = g.Key,
                        Cantidad = g.Sum(x => x.Cantidad)
                    })
                    .ToList();

                var ids = detallesAgrupados.Select(d => d.ID_Producto).ToList();

                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.ID_Producto))
                    .ToDictionaryAsync(p => p.ID_Producto);

                var calculos = new List<(int idProducto, int cantidad, decimal precio, decimal subtotal, decimal iva, decimal ivaPorcentaje)>();

                foreach (var item in detallesAgrupados)
                {
                    if (!productos.ContainsKey(item.ID_Producto))
                        throw new Exception($"Producto no existe (ID: {item.ID_Producto})");

                    var producto = productos[item.ID_Producto];

                    decimal precio = producto.PrecioVTA;
                    decimal ivaPorcentaje = producto.IVA_Porcentaje;

                    decimal subtotal = item.Cantidad * precio;
                    decimal iva = subtotal * (ivaPorcentaje / 100);

                    calculos.Add((item.ID_Producto, item.Cantidad, precio, subtotal, iva, ivaPorcentaje));

                    totalBase += subtotal;
                    totalIVA += iva;
                }

                decimal totalFinal = totalBase + totalIVA;

                // ======================================================
                // 🔹 AJUSTE CONTADO
                // ======================================================
                if (venta.TipoVenta == "CONTADO")
                    venta.AbonoInicial = totalFinal;

                // ======================================================
                // 🔥 VALIDACIONES
                // ======================================================
                if (totalBase <= 0)
                    return BadRequest("El total base no es válido.");

                if (totalFinal <= 0)
                    return BadRequest("El total de la venta no es válido.");

                if (string.IsNullOrWhiteSpace(venta.TipoVenta))
                    return BadRequest("El tipo de venta es obligatorio.");

                // ======================================================
                // 🔥 VALIDAR TIPO CONTADO
                // ======================================================
                if (venta.TipoVenta == "CONTADO")
                {
                    // 🔥 EN CONTADO SIEMPRE SE PAGA TODO
                    venta.AbonoInicial = totalFinal;
                }

                // ======================================================
                // 🔥 VALIDAR TIPO CRÉDITO
                // ======================================================
                else if (venta.TipoVenta == "CREDITO")
                {
                    // 🔥 DEBE EXISTIR ABONO INICIAL
                    if (venta.AbonoInicial <= 0)
                        return BadRequest("Las ventas a crédito requieren abono inicial.");

                    // 🔥 NO PUEDE SUPERAR EL TOTAL
                    if (venta.AbonoInicial > totalFinal)
                        return BadRequest("El abono no puede ser mayor al total.");

                    // 🔥 SI PAGA TODO NO ES CRÉDITO
                    if (venta.AbonoInicial == totalFinal)
                        return BadRequest("Si paga el total la venta debe ser CONTADO.");
                }

                // ======================================================
                // 🔥 TIPO DE VENTA INVÁLIDO
                // ======================================================
                else
                {
                    return BadRequest("Tipo de venta no válido.");
                }

                // ======================================================
                // 🔥 VALIDACIÓN GENERAL
                // ======================================================
                if (venta.AbonoInicial < 0)
                    return BadRequest("El abono no puede ser negativo.");

                // ======================================================
                // 🔥 SALDO
                // ======================================================
                decimal saldo = (venta.TipoVenta == "CONTADO")
                    ? 0
                    : totalFinal - venta.AbonoInicial;

                // ======================================================
                // 🔥 CREAR PEDIDO
                // ======================================================
                var pedido = new Pedido
                {
                   
                    Fecha = DateTime.Now,

                    // 🔥 ESTADO OPERATIVO
                    Estado = "NO DESPACHADO",

                    // 🔥 ESTADO FINANCIERO
                    EstadoPago = saldo == 0 ? "PAGADO" : "ABONADO",
                    ID_Cliente = venta.ID_Cliente,
                    Total = totalBase,
                    TotalIVA = totalIVA,
                    TotalVenta = totalFinal,
                    Saldo = saldo,
                    TipoVenta = venta.TipoVenta
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();



                // ======================================================
                // 🔥 DETALLES
                // ======================================================
                var detalles = new List<DetallePedido>();

                foreach (var c in calculos)
                {
                    var producto = productos[c.idProducto];

                    detalles.Add(new DetallePedido
                    {
                        ID_Pedido = pedido.ID_Pedido,
                        ID_Producto = c.idProducto,
                        Cantidad = c.cantidad,
                        PrecioBase = c.precio,
                        PrecioVenta = c.precio,
                        Subtotal = c.subtotal,
                        IVA_Porcentaje = c.ivaPorcentaje,
                        IVA_Valor = c.iva,
                        //Cantidad_Despachada = 0
                    });
                }
                _context.DetallePedidos.AddRange(detalles);
                await _context.SaveChangesAsync();

                // ======================================================
                // 🔥 ABONO
                // ======================================================

                Abono? abono = null;

                if (venta.AbonoInicial > 0)
                {
                    if (!venta.ID_MetodoPago.HasValue)
                        return BadRequest("Debe seleccionar método de pago.");

                    /*
                    var usuarioNombre =
                      HttpContext.Session.GetString("UsuarioNombre");

                                       

                    if (string.IsNullOrEmpty(usuarioIdStr))
                        return Unauthorized("La sesión del usuario expiró.");

                    int idUsuario = int.Parse(usuarioIdStr);
                    */


                    var usuarioIdStr =
                       HttpContext.Session.GetString("UsuarioID");

                    var usuarioNombre =
                        HttpContext.Session.GetString("UsuarioNombre");

                    // ======================================================
                    // 🔥 VALIDAR SESIÓN
                    // ======================================================

                    if (string.IsNullOrWhiteSpace(usuarioIdStr))
                        return Unauthorized("La sesión del usuario expiró.");

                    if (string.IsNullOrWhiteSpace(usuarioNombre))
                        usuarioNombre = "Sistema";

                    int idUsuario = int.Parse(usuarioIdStr);


                  

                    // ======================================================
                    // 🔥 CREAR ABONO
                    // ======================================================

                    abono = new Abono
                    {
                        ID_Pedido = pedido.ID_Pedido,
                        Fecha_Abono = DateTime.Now,
                        Monto = venta.AbonoInicial,
                        ID_MetodoPago = venta.ID_MetodoPago.Value,
                        ID_Usuario = idUsuario,
                        UsuarioRegistro = usuarioNombre,
                        Activo = true,
                        FechaRegistro = DateTime.Now
                    };

                    _context.Abonos.Add(abono);

                    // Genera ID_Abono en la base de datos
                    await _context.SaveChangesAsync();
                                     

                    abono.NumeroRecibo =
                        $"RC-{DateTime.Now:yyyy-MM}-{abono.ID_Abono:D6}";

                    abono.RutaRecibo =
                        $"ReciboCaja/{abono.NumeroRecibo}.pdf";

                    await _context.SaveChangesAsync();
                                        
                    // ======================================================
                    // 🔥 CONFIRMAR TRANSACCIÓN
                    // ======================================================

                    await transaction.CommitAsync();
                }

                
                // ======================================================
                // 🔥 GENERAR RECIBO DE CAJA
                // ======================================================

                // La venta ya fue confirmada y el abono ya existe.
                // Si ocurre un error al generar el PDF, no debe afectar
                // la transacción ya completada.

                if (abono != null && abono.ID_Abono > 0)
                {
                    try
                    {
                        _reciboCajaService.GenerarPDF(abono.ID_Abono);
                    }
                    catch (Exception ex)
                    {
                        // Registrar error para auditoría y soporte.
                        // La venta NO debe fallar porque el PDF no se pudo generar.

                        Console.WriteLine(
                            $"Error generando recibo PDF para Abono {abono.ID_Abono}: {ex.Message}"
                        );
                    }
                }




                // ======================================================
                // 🔥 URL ORDEN DE PRODUCCIÓN
                // ======================================================

                var urlOrdenProduccion = Url.Action(
                    "GenerarOrdenProduccionPDF",
                    "Ventas",
                    new { idPedido = pedido.ID_Pedido },
                    Request.Scheme
                );

                // ======================================================
                // 🔥 URL RECIBO DE CAJA
                // ======================================================

                string? urlReciboCaja = null;

                if (abono != null)
                {
                    urlReciboCaja = Url.Action(
                        "GenerarReciboCajaPDF",
                        "Abonos",
                        new { idAbono = abono.ID_Abono },
                        Request.Scheme
                    );
                }

                // ======================================================
                // ✅ RESPUESTA FINAL
                // ======================================================

                return Ok(new
                {
                    success = true,
                    idPedido = pedido.ID_Pedido,
                    estado = pedido.EstadoPago,
                    saldo = pedido.Saldo,
                    totalVenta = pedido.TotalVenta,

                    urlOrdenProduccion,
                    urlReciboCaja
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }


        // ==========================================================
        // 🔹 FILTROS BASE
        // ==========================================================
        private async Task CargarFiltrosBaseAsync()
        {
            ViewBag.Generos = await _context.Generos
                .Select(g => new SelectListItem
                {
                    Value = g.ID_Genero.ToString(),
                    Text = g.DescripGenero
                }).ToListAsync();

            ViewBag.Telas = await _context.Telas
                .Where(t => t.Activo)
                .Select(t => new SelectListItem
                {
                    Value = t.ID_Telas.ToString(),
                    Text = t.DescripTela
                }).ToListAsync();

            ViewBag.Colores = await _context.Colores
                .Where(c => c.Activo)
                .Select(c => new SelectListItem
                {
                    Value = c.ID_Color.ToString(),
                    Text = c.Nombre
                }).ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerOrdenProduccion(int idPedido)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            try
            {
                // 🔍 PEDIDO
                var pedido = await _context.Pedidos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ID_Pedido == idPedido);

                if (pedido == null)
                {
                    return Json(new { error = "Pedido no encontrado" });
                }
                var detalles = _context.DetallePedidos
                       .Include(d => d.Producto).ThenInclude(p => p.Talla)
                    .Include(d => d.Producto).ThenInclude(p => p.ColorNav)
                    .Include(d => d.Producto).ThenInclude(p => p.Referencia)
                    .Include(d => d.Producto).ThenInclude(p => p.Genero)   // 🔥 NUEVO
                    .Include(d => d.Producto).ThenInclude(p => p.Tela)     // 🔥 NUEVO
                    .Where(d => d.ID_Pedido == idPedido)
                    .Select(d => new DetalleOrdenVM
                    {
                        ID_Producto = d.ID_Producto, // 🔥 IMPORTANTE
                        Producto = d.Producto.Referencia != null
                        ? d.Producto.Referencia.DescripReferencia
                        : "N/A",

                                            Color = !string.IsNullOrEmpty(d.Producto.ColorSnapshot)
                        ? d.Producto.ColorSnapshot
                        : (d.Producto.ColorNav != null ? d.Producto.ColorNav.Nombre : "N/A"),

                                            Talla = d.Producto.Talla != null
                        ? d.Producto.Talla.DescripTalla
                        : "N/A",


                        Cantidad = d.Cantidad
                        /*
                        Producto = d.Producto.Referencia.DescripReferencia,

                        Color = !string.IsNullOrEmpty(d.Producto.ColorSnapshot)
                            ? d.Producto.ColorSnapshot
                            : d.Producto.ColorNav.Nombre,

                        Talla = d.Producto.Talla.DescripTalla,
                        */

                    })
                    .ToList();


                // 🧾 ORDEN
                var orden = new OrdenProduccionVM
                {
                    IdPedido = pedido.ID_Pedido,
                    Fecha = pedido.Fecha.ToString("yyyy-MM-dd"),
                    Estado = pedido.EstadoPago ?? "",
                    TipoVenta = pedido.TipoVenta ?? "",
                    Detalles = detalles
                };

                return Json(orden);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = "Error interno",
                    detalle = ex.Message
                });
            }
        }
        [HttpGet]
        public IActionResult GenerarOrdenProduccionPDF(int idPedido)
        {
            var pedido = _context.Pedidos
                .FirstOrDefault(p => p.ID_Pedido == idPedido);

            if (pedido == null)
                return NotFound();

            var detalles = _context.DetallePedidos
                .Include(d => d.Producto).ThenInclude(p => p.Talla)
                .Include(d => d.Producto).ThenInclude(p => p.ColorNav)
                .Include(d => d.Producto).ThenInclude(p => p.Referencia)
                .Where(d => d.ID_Pedido == idPedido)
                .OrderBy(d => d.Producto.ID_Referencias)
                .ThenBy(d => d.Producto.ID_Genero)
                .ToList();

            using (var stream = new MemoryStream())
            {
                try
                {
                    using (var writer = new PdfWriter(stream))
                    using (var pdf = new PdfDocument(writer))
                    using (var document = new Document(pdf))
                    {
                        // ==========================================
                        // 🔥 RUTA LOGO
                        // ==========================================
                        var rutaLogo = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "img",
                            "Logo.jpg"
                        );

                        // ==========================================
                        // 🔥 HEADER HORIZONTAL (MEJORADO)
                        // ==========================================
                        var headerTable = new Table(new float[] { 3, 1 }).UseAllAvailableWidth();

                        // 🔹 TEXTO IZQUIERDA (MÁS COMPACTO)
                        var headerText = new Paragraph()
                            .Add(new Text("INDOMABLE\n").SetFontSize(18))
                            .Add(new Text("ORDEN DE PRODUCCION").SetFontSize(12));

                        var cellTexto = new Cell()
                            .Add(headerText)
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                        headerTable.AddCell(cellTexto);

                        // 🔹 LOGO DERECHA (MÁS GRANDE)
                        if (System.IO.File.Exists(rutaLogo))
                        {
                            try
                            {
                                var imageData = ImageDataFactory.Create(rutaLogo);

                                var logo = new Image(imageData)
                                    .ScaleToFit(140, 90) // 🔥 MÁS GRANDE
                                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

                                var cellLogo = new Cell()
                                    .Add(logo)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                    .SetTextAlignment(TextAlignment.RIGHT);

                                headerTable.AddCell(cellLogo);
                            }
                            catch
                            {
                                headerTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            }
                        }
                        else
                        {
                            headerTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        }

                        document.Add(headerTable);

                        // ==========================================
                        // 🔥 INFO PEDIDO EN UNA SOLA LÍNEA
                        // ==========================================
                        var infoPedido = new Paragraph(
                            $"Pedido: {pedido.ID_Pedido}    " +
                            $"Fecha: {pedido.Fecha:yyyy-MM-dd}    " +
                            $"Estado: {pedido.EstadoPago ?? "N/A"} - {pedido.TipoVenta ?? "N/A"}"
                        )
                        .SetFontSize(10); // 🔥 MÁS COMPACTO

                        document.Add(infoPedido);

                        document.Add(new Paragraph(" "));

                        // ==========================================
                        // 🔥 TABLA
                        // ==========================================
                        var table = new Table(new float[] { 2, 2, 2, 1, 1 }).UseAllAvailableWidth();

                        table.AddHeaderCell("Tela");
                        table.AddHeaderCell("Color");
                        table.AddHeaderCell("Talla");
                        table.AddHeaderCell("Docenas");
                        table.AddHeaderCell("Ref");

                        var generos = _context.Generos
                            .ToDictionary(g => g.ID_Genero, g => g.DescripGenero ?? "N/A");

                        var telas = _context.Telas
                            .ToDictionary(t => t.ID_Telas, t => t.DescripTela ?? "N/A");

                        string? productoActual = null;
                        int subtotalProducto = 0;
                        int totalGeneral = 0;

                        var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                        foreach (var d in detalles)
                        {
                            if (d.Producto == null)
                                continue;

                            var producto = d.Producto;

                            var claveProducto = $"{producto.ID_Referencias}_{producto.ID_Genero}";

                            // ==========================================
                            // 🔥 CAMBIO DE PRODUCTO → IMPRIMIR SUBTOTAL
                            // ==========================================
                            if (productoActual != null && productoActual != claveProducto)
                            {
                                var subtotalCell = new Cell(1, 5)
                                    .Add(new Paragraph($"TOTAL: {subtotalProducto} docenas").SetFont(bold));

                                table.AddCell(subtotalCell);

                                subtotalProducto = 0;
                            }

                            // ==========================================
                            // 🔥 NUEVO PRODUCTO
                            // ==========================================
                            if (productoActual != claveProducto)
                            {
                                productoActual = claveProducto;

                                var nombreProductoBase = producto.Referencia?.DescripReferencia ?? "N/A";

                                var genero = generos.ContainsKey(producto.ID_Genero)
                                    ? generos[producto.ID_Genero]
                                    : "N/A";

                                var nombreProducto = $"{nombreProductoBase} - {genero}";

                                var cellProducto = new Cell(1, 5)
                                    .Add(new Paragraph(nombreProducto).SetFont(bold));

                                table.AddCell(cellProducto);
                            }

                            var color = !string.IsNullOrEmpty(producto.ColorSnapshot)
                                ? producto.ColorSnapshot
                                : (producto.ColorNav?.Nombre ?? "N/A");

                            var tela = telas.ContainsKey(producto.ID_Telas)
                                ? telas[producto.ID_Telas]
                                : "N/A";

                            var talla = producto.Talla?.DescripTalla ?? "N/A";

                            table.AddCell(tela);
                            table.AddCell(color);
                            table.AddCell(talla);
                            table.AddCell(d.Cantidad.ToString());
                            table.AddCell("#" + d.ID_Producto);

                            subtotalProducto += d.Cantidad;
                            totalGeneral += d.Cantidad;
                        }

                        // ==========================================
                        // 🔥 ÚLTIMO SUBTOTAL
                        // ==========================================
                        var ultimoSubtotal = new Cell(1, 5)
                            .Add(new Paragraph($"TOTAL: {subtotalProducto} docenas").SetFont(bold));

                        table.AddCell(ultimoSubtotal);

                        // ==========================================
                        // 🔥 TOTAL GENERAL
                        // ==========================================
                        var totalFinal = new Cell(1, 5)
                            .Add(new Paragraph($"TOTAL GENERAL: {totalGeneral} docenas").SetFont(bold));

                        table.AddCell(totalFinal);

                        document.Add(table);

                        // ==========================================
                        // 🔥 OBSERVACIONES (MÁS COMPACTO)
                        // ==========================================
                        document.Add(new Paragraph(" "));
                        document.Add(new Paragraph("Observaciones: _________________________________"));
                        document.Add(new Paragraph(" "));

                        // ==========================================
                        // 🔥 FIRMAS EN UNA SOLA FILA (SIN SALTO)
                        // ==========================================
                        var firmasTable = new Table(new float[] { 1, 1 })
                            .UseAllAvailableWidth()
                            .SetKeepTogether(true); // 🔥 CLAVE

                        firmasTable.AddCell(new Cell()
                            .Add(new Paragraph("Elaboro (Ventas): ____________________"))
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                            .SetPaddingTop(10));

                        firmasTable.AddCell(new Cell()
                            .Add(new Paragraph("Recibio (Produccion): ________________"))
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetPaddingTop(10));

                        document.Add(firmasTable);

                        document.Close();

                    }

                    return File(stream.ToArray(), "application/pdf", $"Orden_{idPedido}.pdf");
                }
                catch (Exception ex)
                {
                    return Content("Error generando PDF: " + ex.Message);
                }
            }
        }

    }

}