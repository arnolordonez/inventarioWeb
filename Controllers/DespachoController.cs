using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Models;
using InventarioWEB.Data;
using InventarioWEB.ViewModels;
//using iText.Commons.Actions.Contexts;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Microsoft.AspNetCore.Hosting;
using iText.IO.Image;
using iText.Layout.Borders;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;

namespace InventarioWEB.Controllers
{
    [AllowAnonymous] // 🔥 IMPORTANTE: permite acceso sin login
    public class DespachoController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly MovimientoVentasDbContext _context;
        public DespachoController(MovimientoVentasDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // ==========================================================
        // LISTADO
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            // 🔥 SOLO LECTURA → AsNoTracking mejora rendimiento
            var despachos = await _context.Despachos
                .Include(d => d.Pedido)
                .OrderBy(d => d.ID_Pedido)
                .ThenBy(d => d.ID_Despacho)
                .AsNoTracking()
                .ToListAsync();

            return View(despachos);
        }

        
        // ==========================================================
        // SELECCIONAR PEDIDO
        // ==========================================================
        public async Task<IActionResult> SeleccionarPedido()
        {
            // 🔥 SOLO PEDIDOS PAGADOS Y NO DESPACHADOS
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
               .Where(p =>
               
                 p.Estado != "DESPACHADO"
                 &&
                 (
                     p.EstadoPago == "PAGADO"
                     || p.EstadoPago == "ABONADO"
                 )
 
               )
                .OrderBy(p => p.ID_Pedido)
                .AsNoTracking()
                .ToListAsync();

            return View(pedidos);
        }
        /*
        public async Task<IActionResult> SeleccionarPedido()
        {
            // 🔥 SOLO PEDIDOS PENDIENTES
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Estado != "DESPACHADO")
                .OrderBy(p => p.ID_Pedido)
                .AsNoTracking()
                .ToListAsync();

            return View(pedidos);
        }
        */
        // ==========================================================
        // DETALLE
        // ==========================================================
        public async Task<IActionResult> Detalle(int id)
        {
            var despacho = await _context.Despachos
                .Include(d => d.Pedido)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Talla)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Referencia)
                .AsNoTracking() // 🔥 SOLO VISUAL
                .FirstOrDefaultAsync(d => d.ID_Despacho == id);

            if (despacho == null)
                return NotFound();

            return View(despacho);
        }

        // ==========================================================
        // CREAR (GET)
        // ==========================================================
        public async Task<IActionResult> Crear(int idPedido)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.DetallePedidos)
                    .ThenInclude(dp => dp.Producto)
                        .ThenInclude(p => p.Talla)
                .FirstOrDefaultAsync(p => p.ID_Pedido == idPedido);

            if (pedido == null)
                return NotFound();

            if (pedido.Estado == "DESPACHADO")
                return BadRequest("El pedido ya está completamente despachado");

            // =====================================================
            // 🔥 FUENTE REAL: detalle_despacho (NO detalle_pedido)
            // =====================================================
            // =====================================================
            // 🔥 PRODUCCIÓN REAL DISPONIBLE PARA DESPACHO
            // =====================================================

            var produccionDisponible = await (
                from dp in _context.DetalleProducciones

                group dp by dp.ID_DetallePedido into g

                select new
                {
                    ID_DetallePedido = g.Key,

                    TotalProducido =
                        g.Sum(x => x.CantidadProducida)
                }
            )
           .ToDictionaryAsync(
                x => x.ID_DetallePedido,
                x => x.TotalProducido
            );

            // =====================================================
            // 🔥 DESPACHADO REAL
            // =====================================================

            var despachadoReal = await _context.DetalleDespachos
                .Join(_context.Despachos,
                    dd => dd.ID_Despacho,
                    d => d.ID_Despacho,
                    (dd, d) => new { dd, d })
                .Where(x => x.d.ID_Pedido == idPedido)
                .GroupBy(x => x.dd.ID_Detalle)
                .Select(g => new
                {
                    DetalleId = g.Key,
                    Total = g.Sum(x => x.dd.Cantidad_Despachada)
                })
                .ToDictionaryAsync(x => x.DetalleId, x => x.Total);

            // =====================================================
            // 🔥 CONSTRUCCIÓN DEL VIEWMODEL
            // =====================================================
            var vm = new DespachoTallaViewModel
            {
                ID_Pedido = pedido.ID_Pedido,

                Tallas = pedido.DetallePedidos.Select(dp =>
                {
                    // =====================================================
                    // 🔥 DESPACHADO REAL
                    // =====================================================

                    var yaDespachado =
                        despachadoReal.ContainsKey(dp.ID_Detalle)
                            ? despachadoReal[dp.ID_Detalle]
                            : 0;

                    // =====================================================
                    // 🔥 PRODUCCIÓN REAL
                    // =====================================================

                    var producido =
                        produccionDisponible.ContainsKey(dp.ID_Detalle)
                            ? produccionDisponible[dp.ID_Detalle]
                            : 0;

                    // =====================================================
                    // 🔥 DISPONIBLE PARA DESPACHO
                    // =====================================================

                    var disponibleDespacho =
                        producido - yaDespachado;

                    if (disponibleDespacho < 0)
                        disponibleDespacho = 0;

                    // =====================================================
                    // 🔥 VIEWMODEL
                    // =====================================================

                    return new DespachoTallaItemVM
                    {
                        ID_Detalle = dp.ID_Detalle,

                        ID_Producto = dp.ID_Producto,

                        Talla = dp.Producto.Talla?.DescripTalla ?? "",

                        CantidadPedida = dp.Cantidad,

                        CantidadDespachada = yaDespachado,

                        CantidadDisponible = disponibleDespacho
                    };

                }).ToList(),

                // 🔥 TOTAL ORIGINAL DEL PEDIDO
                TotalDocenasPedido = pedido.DetallePedidos.Sum(x => x.Cantidad)
            };
                    // =====================================================
                    // 🔴 VALIDACIÓN: NO HAY PRODUCTOS DISPONIBLES
                    // =====================================================

                    var totalDisponible = vm.Tallas.Sum(t => t.CantidadDisponible);

                    if (totalDisponible <= 0)
                    {
                        ViewBag.Mensaje = "No hay productos a despachar";
                    }

            return View(vm);
        }

        
        // ==========================================================
        // CREAR (POST)
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DespachoTallaViewModel model)
        {
            if (!ModelState.IsValid)
                return await Crear(model.ID_Pedido);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.DetallePedidos)
                    .FirstOrDefaultAsync(p => p.ID_Pedido == model.ID_Pedido);

                if (pedido == null)
                    throw new Exception("El pedido no existe");

                // =====================================================
                // 🔥 VALIDACIÓN FINANCIERA DEL DESPACHO
                // =====================================================

                // 🔹 TOTAL ABONADO
                decimal totalAbonado = await _context.Abonos
                    .Where(a =>
                        a.ID_Pedido == pedido.ID_Pedido
                        && a.Activo
                    )
                    .SumAsync(a => (decimal?)a.Monto) ?? 0;


                // 🔹 TOTAL YA DESPACHADO EN DINERO (SIN IVA)
                decimal totalDespachadoBase = await _context.DetalleDespachos
                    .Join(_context.Despachos,
                        dd => dd.ID_Despacho,
                        d => d.ID_Despacho,
                        (dd, d) => new { dd, d })
                    .Where(x => x.d.ID_Pedido == pedido.ID_Pedido)
                    .Join(_context.DetallePedidos,
                        x => x.dd.ID_Detalle,
                        dp => dp.ID_Detalle,
                        (x, dp) => new
                        {
                            Cantidad = x.dd.Cantidad_Despachada,
                            Precio = dp.PrecioVenta
                        })
                    .SumAsync(x => x.Cantidad * x.Precio);

                // 🔹 IVA PROPORCIONAL
                decimal porcentajeIVA = pedido.Total > 0
                    ? (pedido.TotalIVA / pedido.Total)
                    : 0;

                // 🔹 TOTAL YA DESPACHADO CON IVA
                decimal totalDespachadoConIVA =
                    totalDespachadoBase * (1 + porcentajeIVA);

                // 🔹 TOTAL NUEVO DESPACHO (SIN IVA)
                decimal totalNuevoDespacho = model.Tallas
                    .Where(t => t.Cantidad > 0)
                    .Join(
                        pedido.DetallePedidos,
                        t => t.ID_Detalle,
                        dp => dp.ID_Detalle,
                        (t, dp) => t.Cantidad * dp.PrecioVenta
                    )
                    .Sum();

                // 🔹 TOTAL NUEVO DESPACHO CON IVA
                decimal totalNuevoDespachoConIVA =
                    totalNuevoDespacho * (1 + porcentajeIVA);

                // 🔹 TOTAL ACUMULADO REAL
                decimal totalAcumuladoDespacho =
                    totalDespachadoConIVA + totalNuevoDespachoConIVA;

                // 🚫 VALIDACIÓN FINANCIERA
                if (pedido.TipoVenta == "CREDITO"
                    && totalAcumuladoDespacho > totalAbonado)
                {
                    decimal disponible =
                        totalAbonado - totalDespachadoConIVA;

                    if (disponible < 0)
                        disponible = 0;

                    throw new Exception(
                        $"El cliente solo tiene disponible para despacho: ${disponible:N0}. " +
                        $"No puede despachar mercancía por ${totalNuevoDespachoConIVA:N0}."
                    );
                }

                // 🔥 VALIDAR QUE EL PEDIDO NO ESTÉ COMPLETAMENTE DESPACHADO
                if (pedido.Estado == "DESPACHADO")
                    throw new Exception("El pedido ya fue despachado completamente");

                // 🔥 SOLO SE PUEDEN DESPACHAR PEDIDOS NO DESPACHADOS
                if (pedido.Estado != "NO DESPACHADO")
                    throw new Exception(
                        $"El pedido no puede despacharse en estado: {pedido.Estado}"
                    );

                if (model.Tallas == null || !model.Tallas.Any())
                    throw new Exception("No hay datos para despachar");

                if (model.Tallas.Any(t => t.Cantidad < 0))
                    throw new Exception("No se permiten valores negativos en las cantidades");
            

                // =====================================================
                // 🔥 PRODUCTOS (DESDE DETALLE_PEDIDO → FUENTE CORRECTA)
                // =====================================================
                var productoIds = pedido.DetallePedidos
                    .Select(d => d.ID_Producto)
                    .Distinct()
                    .ToList();

                var productos = await _context.Productos
                    .Where(p => productoIds.Contains(p.ID_Producto))
                    .ToDictionaryAsync(p => p.ID_Producto);

                // =====================================================
                // 🔥 HISTÓRICO REAL (FUENTE: detalle_despacho)
                // =====================================================
                var despachadoReal = await _context.DetalleDespachos
                    .Join(_context.Despachos,
                        dd => dd.ID_Despacho,
                        d => d.ID_Despacho,
                        (dd, d) => new { dd, d })
                    .Where(x => x.d.ID_Pedido == model.ID_Pedido)
                    .GroupBy(x => x.dd.ID_Detalle)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Sum(x => x.dd.Cantidad_Despachada)
                    );

                // =====================================================
                // 🔥 PRODUCCIÓN REAL DISPONIBLE
                // =====================================================

                var produccionDisponible = await _context.DetalleProducciones
                    .GroupBy(x => x.ID_DetallePedido)
                    .Select(g => new
                    {
                        ID_DetallePedido = g.Key,
                        TotalProducido = g.Sum(x => x.CantidadProducida)
                    })
                    .ToDictionaryAsync(
                        x => x.ID_DetallePedido,
                        x => x.TotalProducido
                    );

                // =====================================================
                // 🔥 VALIDACIÓN POR ITEM
                // =====================================================
                foreach (var item in model.Tallas.Where(t => t.Cantidad > 0))
                {
                    // =================================================
                    // 🔍 DETALLE PEDIDO
                    // =================================================

                    var detalle = pedido.DetallePedidos
                        .FirstOrDefault(d => d.ID_Detalle == item.ID_Detalle);

                    if (detalle == null)
                    {
                        throw new Exception(
                            "El detalle no pertenece al pedido."
                        );
                    }

                    // =================================================
                    // 🔍 PRODUCTO
                    // =================================================

                    if (!productos.TryGetValue(detalle.ID_Producto, out var producto))
                    {
                        throw new Exception(
                            $"Producto inválido ID {detalle.ID_Producto}"
                        );
                    }

                    // =================================================
                    // 🚚 YA DESPACHADO
                    // =================================================

                    var yaDespachado =
                        despachadoReal.ContainsKey(detalle.ID_Detalle)
                            ? despachadoReal[detalle.ID_Detalle]
                            : 0;

                    // =================================================
                    // 🏭 PRODUCCIÓN REAL
                    // =================================================

                    var producido =
                        produccionDisponible.ContainsKey(detalle.ID_Detalle)
                            ? produccionDisponible[detalle.ID_Detalle]
                            : 0;

                    // =================================================
                    // 📦 DISPONIBLE PARA DESPACHO
                    // =================================================

                    var disponibleProduccion =
                        producido - yaDespachado;

                    if (disponibleProduccion < 0)
                    {
                        disponibleProduccion = 0;
                    }

                    // =================================================
                    // 🚫 VALIDAR PRODUCCIÓN DISPONIBLE
                    // =================================================

                    if (item.Cantidad > disponibleProduccion)
                    {
                        throw new Exception(
                            $"No existe producción disponible para despachar " +
                            $"en talla {item.Talla}. " +
                            $"Disponible producción: {disponibleProduccion} docenas."
                        );
                    }

                    // =================================================
                    // 🚫 VALIDACIÓN STOCK FÍSICO
                    // =================================================

                    if (item.Cantidad > disponibleProduccion)
                    {
                        throw new Exception(
                            $"No existe producción disponible para despachar " +
                            $"en talla {item.Talla}. " +
                            $"Disponible producción: {disponibleProduccion} docenas."
                        );
                    }
                    // =================================================
                    // ⚠️ STOCK BAJO
                    // =================================================

                    if (producto.Stock < 10)
                    {
                        ModelState.AddModelError(
                            "",
                            $"⚠️ Stock bajo en {producto.Nombre} " +
                            $"(talla {item.Talla}). " +
                            $"Disponible: {producto.Stock}"
                        );
                    }
                }

                // =====================================================
                // =====================================================

                // 🔥 VALIDACIÓN GLOBAL (ACUMULADA POR PRODUCTO)
                var agrupadoPorProducto = model.Tallas
                    .Where(t => t.Cantidad > 0)
                    .GroupBy(t => pedido.DetallePedidos
                        .First(d => d.ID_Detalle == t.ID_Detalle).ID_Producto)
                    .Select(g => new
                    {
                        ProductoId = g.Key,
                        Total = g.Sum(x => x.Cantidad)
                    });

                foreach (var grupo in agrupadoPorProducto)
                {
                    var producto = productos[grupo.ProductoId];

                    if (grupo.Total > producto.Stock)
                    {
                        throw new Exception(
                            $"Stock insuficiente acumulado para producto {producto.Nombre}. Disponible: {producto.Stock}, solicitado: {grupo.Total}"
                        );
                    }
                }

                // =====================================================
                // 🔥 VALIDACIÓN GLOBAL DEL PEDIDO
                // =====================================================

                var pedidoTotal = pedido.DetallePedidos.Sum(x => x.Cantidad);

                var totalNuevo = model.Tallas.Sum(t => t.Cantidad);

                var totalYaDespachado = despachadoReal.Values.Sum();

                if ((totalNuevo + totalYaDespachado) > pedidoTotal)
                {
                    throw new Exception("No puede despachar más de lo pendiente");
                }

                // =====================================================
                // VALIDACIÓN FINAL
                // =====================================================
                if ((totalNuevo + totalYaDespachado) > pedidoTotal)
                {
                    throw new Exception("No puede despachar más de lo pendiente");
                }

                // =====================================================
                // CREAR DESPACHO
                // =====================================================
                var despacho = new Despacho
                {
                    ID_Pedido = model.ID_Pedido,
                    Fecha = DateTime.Now,
                    Estado = EstadoDespacho.Despachado,
                    Tipo = TipoDespacho.Parcial // se recalcula después
                };

                _context.Despachos.Add(despacho);
                await _context.SaveChangesAsync();

                // =====================================================
                // DETALLES (FUENTE REAL: detalle_despacho)
                // =====================================================
                foreach (var item in model.Tallas.Where(t => t.Cantidad > 0))
                {
                    // ==============================================
                    // 🔍 OBTENER DETALLE DEL PEDIDO
                    // ==============================================
                    var detallePedido = pedido.DetallePedidos
                        .FirstOrDefault(d => d.ID_Detalle == item.ID_Detalle);

                    if (detallePedido == null)
                        throw new Exception("El detalle no pertenece al pedido");

                    // ==============================================
                    // 🔍 OBTENER PRODUCTO
                    // ==============================================
                    if (!productos.TryGetValue(detallePedido.ID_Producto, out var producto))
                        throw new Exception($"Producto no encontrado ID {detallePedido.ID_Producto}");

                    // ==============================================
                    // 🔒 VALIDACIONES FINALES (ANTI-CONCURRENCIA)
                    // ==============================================
                    if (item.Cantidad <= 0)
                        continue;

                    if (producto.Stock <= 0)
                        throw new Exception($"Stock sin existencias para el producto: {producto.Nombre}");

                    if (producto.Stock < item.Cantidad)
                    {
                        throw new Exception(
                            $"Stock insuficiente al guardar. Producto: {producto.Nombre}, Disponible: {producto.Stock}"
                        );
                    }

                    // ==============================================
                    // 📦 INSERTAR DETALLE DESPACHO (FUENTE DE VERDAD)
                    // ==============================================
                    var detalleDespacho = new DetalleDespacho
                    {
                        ID_Despacho = despacho.ID_Despacho,
                        ID_Detalle = detallePedido.ID_Detalle,
                        ID_Producto = detallePedido.ID_Producto,
                        Cantidad_Despachada = item.Cantidad
                    };

                    _context.DetalleDespachos.Add(detalleDespacho);

                    // ==============================================
                    // 📉 ACTUALIZAR STOCK (ÚNICA MUTACIÓN REAL)
                    // ==============================================
                    producto.Stock -= item.Cantidad;

                    if (producto.Stock < 0)
                    {
                        throw new Exception(
                            $"El stock no puede quedar negativo. Producto: {producto.Nombre}"
                        );
                    }

                    _context.Entry(producto).State = EntityState.Modified;
                }
                // ==============================================
                // 💾 GUARDAR CAMBIOS
                // ==============================================
                await _context.SaveChangesAsync();

                // =====================================================
                // 🔥 RECALCULAR ESTADO REAL DEL PEDIDO (BD)
                // =====================================================
                var totalPedidoFinal = pedido.DetallePedidos.Sum(x => x.Cantidad);

                var totalDespachado = await _context.DetalleDespachos
                    .Join(_context.Despachos,
                        dd => dd.ID_Despacho,
                        d => d.ID_Despacho,
                        (dd, d) => new { dd, d })
                    .Where(x => x.d.ID_Pedido == model.ID_Pedido)
                    .SumAsync(x => x.dd.Cantidad_Despachada);

                // =====================================================
                // 🔥 DEFINIR TIPO DE DESPACHO
                // =====================================================
                despacho.Tipo = totalDespachado >= totalPedidoFinal
                    ? TipoDespacho.Completo
                    : TipoDespacho.Parcial;

                // =====================================================
                // 🔥 ACTUALIZAR ESTADO DEL PEDIDO (IMPORTANTE)
                // =====================================================
                if (totalDespachado >= totalPedidoFinal)
                {
                    pedido.Estado = "DESPACHADO";
                    _context.Entry(pedido).State = EntityState.Modified;
                }

                // ==============================================
                // 💾 GUARDAR ESTADO FINAL
                // ==============================================
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 🚀 CAMBIO CLAVE: IR A FACTURA EN LUGAR DE INDEX
                return RedirectToAction("Factura", new { id = despacho.ID_Despacho });

            }
                       
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError("", ex.Message);

                return await Crear(model.ID_Pedido);
                // return RedirectToAction(nameof(Crear), new { idPedido = model.ID_Pedido });
            }
            
        }

        // ==========================================================
        // 🔥 GENERAR FACTURA PDF
        // ==========================================================
        public async Task<IActionResult> Factura(int id)
        {
            var despacho = await _context.Despachos
                .Include(d => d.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Talla)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Referencia)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.ColorNav)
                .FirstOrDefaultAsync(d => d.ID_Despacho == id);

            if (despacho == null)
                return NotFound();

            using var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // ======================================================
            // 🔥 FUENTES
            // ======================================================
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // ======================================================
            // 🔥 LOGO
            // ======================================================
            var logoPath = Path.Combine(_env.WebRootPath, "img", "Logo.jpg");

            Image? logo = null;

            if (System.IO.File.Exists(logoPath))
            {
                var imageData = ImageDataFactory.Create(logoPath);

                // 🔥 LOGO MÁS GRANDE
                logo = new Image(imageData).ScaleToFit(160, 100);
            }

            // ======================================================
            // 🔥 ENCABEZADO
            // ======================================================
            var headerTable = new Table(new float[] { 1, 3 })
                .UseAllAvailableWidth();

            var cellLogo = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            if (logo != null)
            {
                cellLogo.Add(logo);
            }

            headerTable.AddCell(cellLogo);

            headerTable.AddCell(new Cell()
                .Add(new Paragraph("INDOMABLE S.A.S").SetFont(boldFont).SetFontSize(12))
                .Add(new Paragraph("NIT: 900.123.456-7").SetFont(normalFont))
                .Add(new Paragraph("Bogotá D.C").SetFont(normalFont))
                .Add(new Paragraph("Tel: 300 123 4567").SetFont(normalFont))
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            );

            document.Add(headerTable);

            document.Add(new Paragraph("\n"));

            // ======================================================
            // 🔥 DATOS FACTURA / TRAZABILIDAD
            // ======================================================
            document.Add(new Paragraph($"Factura N°: {despacho.ID_Despacho}").SetFont(boldFont));
            document.Add(new Paragraph($"Pedido N°: {despacho.ID_Pedido}").SetFont(boldFont));
            document.Add(new Paragraph($"Fecha: {despacho.Fecha:dd/MM/yyyy HH:mm}"));
            document.Add(new Paragraph($"Estado: {despacho.Estado}"));
            document.Add(new Paragraph($"Tipo: {despacho.Tipo}"));

            document.Add(new Paragraph("\n"));

            // ======================================================
            // 🔥 CLIENTE
            // ======================================================
            var cliente = despacho.Pedido.Cliente;

            document.Add(new Paragraph(
                $"Cliente: {cliente.Nombre} {cliente.Apellido}    " +
                $"Doc: {cliente.ID_Cliente}    Tel: {cliente.Telefono}"
            ));

            document.Add(new Paragraph(
                $"Dirección: {cliente.Direccion}    Ciudad: {cliente.CiudadMunicipio}"
            ));

            document.Add(new Paragraph("\n"));

            // ======================================================
            // 🔥 TABLA PRODUCTOS
            // ======================================================
            var table = new Table(new float[] { 2, 4, 2, 2, 2, 2 });
            table.UseAllAvailableWidth();

            table.AddHeaderCell(new Cell().Add(new Paragraph("Cod").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Producto").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Talla").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Color").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Cant").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal").SetFont(boldFont)));

            decimal total = 0;

            var detallesIds = despacho.Detalles.Select(x => x.ID_Detalle).ToList();

            var precios = await _context.DetallePedidos
                .Where(x => detallesIds.Contains(x.ID_Detalle))
                .ToDictionaryAsync(x => x.ID_Detalle, x => x.PrecioVenta);

            foreach (var d in despacho.Detalles)
            {
                var p = d.Producto;

                var color = p.ColorNav?.Nombre ?? p.ColorSnapshot ?? "";

                decimal precio = precios.ContainsKey(d.ID_Detalle)
                    ? precios[d.ID_Detalle]
                    : 0;

                decimal subtotal = precio * d.Cantidad_Despachada;

                total += subtotal;

                table.AddCell(new Paragraph(p.ID_Producto.ToString()));
                table.AddCell(new Paragraph(p.Nombre));
                table.AddCell(new Paragraph(p.Talla?.DescripTalla ?? ""));
                table.AddCell(new Paragraph(color));
                table.AddCell(new Paragraph(d.Cantidad_Despachada.ToString()));
                table.AddCell(new Paragraph($"${subtotal:N0}"));
            }

            document.Add(table);

            document.Add(new Paragraph("\n"));


            // ======================================================
            // 🔥 CONTEXTO DE FACTURA (CLARO Y PROFESIONAL)
            // ======================================================

            // ======================================================
            // 🔥 CONTEXTO DE FACTURA
            // ======================================================

            var boldFontSmall = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            var pedido = despacho.Pedido;

            document.Add(new Paragraph("\n"));
            
            // ======================================================
            // 🔥 TOTALES DEL DESPACHO
            // ======================================================

            // IVA proporcional
            decimal porcentajeIVA = pedido.Total > 0
                ? (pedido.TotalIVA / pedido.Total)
                : 0;

            var iva = total * porcentajeIVA;
            var totalFinal = total + iva;

            // 🔹 RESUMEN DESPACHO
            document.Add(new Paragraph("RESUMEN DEL DESPACHO")
                .SetFont(boldFontSmall)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph($"Subtotal: ${total:N0}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph($"IVA ({porcentajeIVA:P0}): ${iva:N0}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph($"Total del despacho: ${totalFinal:N0}")
                .SetFont(boldFontSmall)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.RIGHT));

            // 🔹 NOTA DE PAGO
            if (pedido.TipoVenta == "CONTADO" && pedido.Saldo == 0)
            {
                document.Add(
                    new Paragraph("Pedido pagado anticipadamente")
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.RIGHT)
                );
            }

            document.Add(new Paragraph("\n"));


            // ======================================================
            // 🔥 HISTÓRICO DE DESPACHOS
            // ======================================================

            var totalDespachadoPrevio = await _context.DetalleDespachos
                .Join(_context.Despachos,
                    dd => dd.ID_Despacho,
                    d => d.ID_Despacho,
                    (dd, d) => new { dd, d })
                .Where(x => x.d.ID_Pedido == pedido.ID_Pedido
                         && x.d.ID_Despacho != despacho.ID_Despacho)
                .Join(_context.DetallePedidos,
                    x => x.dd.ID_Detalle,
                    dp => dp.ID_Detalle,
                    (x, dp) => new
                    {
                        Cantidad = x.dd.Cantidad_Despachada,
                        Precio = dp.PrecioVenta
                    })
                .SumAsync(x => x.Cantidad * x.Precio);

            // convertir a total con IVA
            var totalPrevioConIVA = totalDespachadoPrevio * (1 + porcentajeIVA);
            var totalAcumulado = totalPrevioConIVA + totalFinal;

            // 🔹 CALCULAR PENDIENTE
            var pendiente = pedido.TotalVenta - totalAcumulado;


            // ======================================================
            // 🔥 ESTADO DEL PEDIDO (CLAVE)
            // ======================================================

            document.Add(new Paragraph("ESTADO DEL PEDIDO")
                .SetFont(boldFontSmall)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph($"Total del pedido: ${pedido.TotalVenta:N0}")
                .SetTextAlignment(TextAlignment.RIGHT));

            if (totalDespachadoPrevio == 0)
            {
                // 🟢 PRIMER DESPACHO (SIN REDUNDANCIA)
                document.Add(new Paragraph("Inicio de despacho del pedido")
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(new Paragraph($"Total despachado: ${totalAcumulado:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));
            }
            else
            {
                // 🟡 DESPACHOS POSTERIORES
                document.Add(new Paragraph($"Despachado previamente: ${totalPrevioConIVA:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(new Paragraph($"Despachado en este documento: ${totalFinal:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(new Paragraph($"Total despachado: ${totalAcumulado:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));
            }


            if (pendiente > 0)
            {
                document.Add(new Paragraph($"Pendiente por despachar: ${pendiente:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));
            }

            // 🔹 ESTADO LOGÍSTICO
            string estadoLogistico = pendiente == 0 ? "COMPLETO" : "PARCIAL";

            document.Add(new Paragraph($"Estado del pedido: {estadoLogistico}")
                .SetFont(boldFontSmall)
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("\n"));


            // ======================================================
            // 🔥 ESTADO DE PAGO (SEPARADO)
            // ======================================================

            document.Add(new Paragraph("ESTADO DE PAGO")
                .SetFont(boldFontSmall)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT));
            string estadoPago = pedido.Saldo == 0 ? "PAGADO" : "ABONADO";

            document.Add(new Paragraph($"Tipo de venta: {pedido.TipoVenta}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph($"Estado de pago: {estadoPago}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph($"Saldo financiero: ${pedido.Saldo:N0}")
                .SetTextAlignment(TextAlignment.RIGHT));


            // ======================================================
            // 🔥 FIRMA
            // ======================================================

            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph("____________________________"));
            document.Add(new Paragraph("Firma Responsable"));

            document.Close();

            return File(stream.ToArray(), "application/pdf", $"Factura_{id}.pdf");
        }
    }
}