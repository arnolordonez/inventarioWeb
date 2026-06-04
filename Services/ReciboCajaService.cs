using InventarioWEB.Data;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using iText.Layout;


namespace InventarioWEB.Services
{
    public class ReciboCajaService
    {
        private readonly MovimientoVentasDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ReciboCajaService(
            MovimientoVentasDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public void GenerarPDF(int idAbono)
        {
            var abono = _context.Abonos
                .Include(a => a.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Include(a => a.MetodoPago)
                .FirstOrDefault(a => a.ID_Abono == idAbono);

            if (abono == null)
                throw new Exception($"No existe el abono {idAbono}");

            if (abono.Pedido == null)
                throw new Exception("El abono no tiene un pedido asociado.");

            var cliente = abono.Pedido.Cliente;

            if (cliente == null)
                throw new Exception("El pedido no tiene un cliente asociado.");

            // ======================================================
            // GENERAR NÚMERO DE RECIBO
            // ======================================================

            if (string.IsNullOrWhiteSpace(abono.NumeroRecibo))
            {
                abono.NumeroRecibo =
                    $"RC-{DateTime.Now:yyyy-MM}-{abono.ID_Abono:D6}";
            }

            // ======================================================
            // CARPETA DESTINO
            // ======================================================

            var carpeta = Path.Combine(
                _env.WebRootPath,
                "ReciboCaja");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var nombreArchivo =
                $"{abono.NumeroRecibo}.pdf";

            var rutaFisica = Path.Combine(
                carpeta,
                nombreArchivo);

            // ======================================================
            // GUARDAR RUTA   ojo hay que revisar en ventas
            // ======================================================
              abono.RutaRecibo =
              $"ReciboCaja/{nombreArchivo}";



           // _context.SaveChanges();

            // ======================================================
            // PDF
            // ======================================================

            using var writer = new PdfWriter(rutaFisica);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var bold = PdfFontFactory.CreateFont(
                StandardFonts.HELVETICA_BOLD);

            // ======================================================
            // ENCABEZADO EMPRESA
            // ======================================================

            document.Add(
                new Paragraph("INDOMABLE S.A.S")
                    .SetFont(bold)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(
                new Paragraph("NIT: 900.123.456-7")
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(
                new Paragraph("Bogotá D.C")
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(
                new Paragraph("Tel: 300 123 4567")
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph(" "));

            document.Add(
                new Paragraph("RECIBO DE CAJA")
                    .SetFont(bold)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph(" "));

            // ======================================================
            // DATOS DEL RECIBO
            // ======================================================

            document.Add(
                new Paragraph($"Recibo No: {abono.NumeroRecibo}")
                    .SetFont(bold));

            document.Add(
                new Paragraph(
                    $"Fecha: {abono.Fecha_Abono:dd/MM/yyyy HH:mm}"
                ));

            document.Add(new Paragraph(" "));

            // ======================================================
            // CLIENTE
            // ======================================================

            document.Add(
                new Paragraph("DATOS DEL CLIENTE")
                    .SetFont(bold));

            document.Add(
                new Paragraph(
                    $"Cliente: {cliente.Nombre} {cliente.Apellido}              Documento: {abono.Pedido.ID_Cliente}"
                ));

            document.Add(
                new Paragraph(
                    $"Teléfono: {cliente.Telefono ?? "N/A"}               Correo: {cliente.Correo ?? "N/A"}"
                ));

            document.Add(new Paragraph(" "));

            // ======================================================
            // PEDIDO
            // ======================================================

            document.Add(
                new Paragraph("INFORMACIÓN DEL PEDIDO")
                    .SetFont(bold));

            document.Add(
                new Paragraph(
                    $"Pedido No: {abono.ID_Pedido}                         Tipo Venta: {abono.Pedido.TipoVenta}"
                ));

            document.Add(
                new Paragraph(
                    $"Estado Financiero: {abono.Pedido.EstadoPago}        Estado Operativo: {abono.Pedido.Estado}"
                ));

            document.Add(new Paragraph(" "));

            // ======================================================
            // MOVIMIENTO FINANCIERO
            // ======================================================

            document.Add(
                new Paragraph("MOVIMIENTO FINANCIERO")
                    .SetFont(bold));

            document.Add(
                new Paragraph(
                    $"Subtotal: ${abono.Pedido.Total:N0}"
                ));

            document.Add(
                new Paragraph(
                    $"IVA: ${abono.Pedido.TotalIVA:N0}"
                ));

            document.Add(
                new Paragraph(
                    $"Total Venta: ${abono.Pedido.TotalVenta:N0}"
                ));

            document.Add(
                new Paragraph(
                    $"Abono Recibido: ${abono.Monto:N0}"
                ));

            document.Add(
                new Paragraph(
                    $"Saldo Pendiente: ${abono.Pedido.Saldo:N0}"
                ));

            document.Add(new Paragraph(" "));

            // ======================================================
            // MÉTODO DE PAGO
            // ======================================================

            document.Add(
                new Paragraph("MÉTODO DE PAGO")
                    .SetFont(bold));

            document.Add(
                new Paragraph(
                    abono.MetodoPago?.Nombre ?? "N/A"
                ));

            document.Add(new Paragraph(" "));

            // ======================================================
            // RESPONSABLE
            // ======================================================

            document.Add(
                new Paragraph("RECIBIDO POR")
                    .SetFont(bold));

            document.Add(
                new Paragraph(
                    abono.UsuarioRegistro ?? "Sistema"
                ));

            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));

            // ======================================================
            // FIRMA
            // ======================================================

            document.Add(
                new Paragraph(
                    "________________________________________"
                )
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(
                new Paragraph(
                    "Firma Responsable"
                )
                .SetTextAlignment(TextAlignment.CENTER));

            document.Close();
        }
    }
}