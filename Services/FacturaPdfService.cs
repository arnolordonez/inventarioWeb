using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using InventarioWEB.Data;
using Microsoft.EntityFrameworkCore;

using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using InventarioWEB.Constants;

namespace InventarioWEB.Services
    {
        public class FacturaPdfService
        {
            // ==========================================================
            // DEPENDENCIAS
            // ==========================================================

            private readonly MovimientoVentasDbContext _context;
            private readonly IWebHostEnvironment _env;

            // ==========================================================
            // CONSTRUCTOR
            // ==========================================================

            public FacturaPdfService(
                MovimientoVentasDbContext context,
                IWebHostEnvironment env)
            {
                _context = context;
                _env = env;
            }

        // NUEVO METODO
        public async Task<byte[]> GenerarFacturaPdfAsync(int id)
        {
            // TODO:
            // Aquí se moverá exactamente el código que hoy
            // existe dentro del método Factura().
            //
            // Este método devolverá:
            //
            // return stream.ToArray();
            //
            // En el siguiente paso trasladaremos el contenido
            // completo sin modificar la lógica.
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
            {
                throw new Exception("No se encontró el despacho solicitado.");
            }

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
                 .Add(new Paragraph(ReportesConstantes.Empresa)
                     .SetFont(boldFont)
                     .SetFontSize(12))

                 .Add(new Paragraph($"NIT: {ReportesConstantes.Nit}")
                     .SetFont(normalFont))

                 .Add(new Paragraph(ReportesConstantes.Ciudad)
                     .SetFont(normalFont))

                 .Add(new Paragraph($"Tel: {ReportesConstantes.Telefono}")
                     .SetFont(normalFont))

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

            document.Add(new Paragraph($"Estado del despacho: {despacho.Estado}")
                .SetFont(normalFont));

            document.Add(new Paragraph($"Tipo de despacho: {despacho.Tipo}")
                .SetFont(normalFont));

            document.Add(new Paragraph("\n"));

            // ======================================================
            // 🔥 CLIENTE
            // ======================================================

            var cliente = despacho.Pedido.Cliente;

            document.Add(
                new Paragraph(
                    $"Cliente: {cliente.Nombre} {cliente.Apellido}      " +
                    $"Documento: {cliente.ID_Cliente}      " +
                    $"Teléfono: {cliente.Telefono}")
                .SetFont(normalFont)
            );

            document.Add(
                new Paragraph(
                    $"Dirección: {cliente.Direccion}      " +
                    $"Ciudad: {cliente.CiudadMunicipio}")
                .SetFont(normalFont)
            );

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

            document.Add(new Paragraph($"Subtotal.................... ${total:N0}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph($"IVA ({porcentajeIVA:P0})............... ${iva:N0}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(
                new Paragraph("────────────────────────────────")
                    .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(
                new Paragraph($"TOTAL A PAGAR......... ${totalFinal:N0}")
                    .SetFont(boldFontSmall)
                    .SetFontSize(12)
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

            document.Add(new Paragraph($"Total del pedido.............. ${pedido.TotalVenta:N0}")
                .SetTextAlignment(TextAlignment.RIGHT));

            if (totalDespachadoPrevio == 0)
            {
                document.Add(new Paragraph($"Inicio del despacho.......... ${totalAcumulado:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));
            }
            else
            {
                document.Add(new Paragraph($"Despachado previamente........ ${totalPrevioConIVA:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(new Paragraph($"En este despacho.............. ${totalFinal:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(new Paragraph($"Total despachado.............. ${totalAcumulado:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));
            }

            if (pendiente > 0)
            {
                document.Add(new Paragraph($"Pendiente..................... ${pendiente:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));
            }

            string estadoLogistico = pendiente == 0 ? "COMPLETO" : "PARCIAL";

            document.Add(
                new Paragraph($"Estado: {estadoLogistico}")
                    .SetFont(boldFontSmall)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("\n"));

            // ======================================================
            // 🔥 ESTADO DE PAGO (AGRUPADO)
            // ======================================================

            string estadoPago = pedido.Saldo == 0 ? "PAGADO" : "ABONADO";

            var bloqueEstadoPago = new Div()
                .SetKeepTogether(true);

            bloqueEstadoPago.Add(
                new Paragraph("ESTADO DE PAGO")
                    .SetFont(boldFontSmall)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT));

            bloqueEstadoPago.Add(
                new Paragraph($"Tipo de venta: {pedido.TipoVenta}")
                    .SetTextAlignment(TextAlignment.RIGHT));

            bloqueEstadoPago.Add(
                new Paragraph($"Estado financiero: {estadoPago}")
                    .SetTextAlignment(TextAlignment.RIGHT));

            bloqueEstadoPago.Add(
                new Paragraph($"Saldo: ${pedido.Saldo:N0}")
                    .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(bloqueEstadoPago);

            // ======================================================
            // 🔥 FIRMA
            // ======================================================
            // ======================================================
            // 🔥 AUTORIZACIÓN INSTITUCIONAL
            // ======================================================

            document.Add(new Paragraph("\n"));

            document.Add(
                new Paragraph("Autorizado por")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(
                new Paragraph("CONFECCIONES INDOMABLE S.A.S.")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(
                new Paragraph("Departamento de Logística")
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            document.Add(
                new Paragraph("Documento generado automáticamente por ERP InventarioWEB.")
                    .SetFont(normalFont)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER));
            // ======================================================
            // FINALIZAR DOCUMENTO PDF
            // ======================================================

            document.Close();

            return stream.ToArray();

        }
    }
}
