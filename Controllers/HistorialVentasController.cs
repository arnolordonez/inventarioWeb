using ClosedXML.Excel;
using InventarioWEB.Data;
using InventarioWEB.Filters;
using InventarioWEB.Pdf;
using InventarioWEB.Services;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using QuestPDF.Fluent;

namespace InventarioWEB.Controllers
{
    [ValidarSesion]
    public class HistorialVentasController : Controller
    {
        private readonly HistorialVentasService _ventasService;
        private readonly MovimientoVentasDbContext _context;
                
        private readonly IEmailService _emailService;

        private readonly CorreoEnviadoService _correoEnviadoService;


        public HistorialVentasController(
            HistorialVentasService ventasService,
            MovimientoVentasDbContext context,
            IEmailService emailService,
            CorreoEnviadoService correoEnviadoService)
        {
            _ventasService = ventasService;
            _context = context;
            _emailService = emailService;
            _correoEnviadoService = correoEnviadoService;
        }

        // =========================================================
        // 🛒 HISTORIAL DE VENTAS
        // =========================================================
        public async Task<IActionResult> Index(VentaHistorialFiltroVM filtro)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? "";

            if (rol != "Administrador" &&
                rol != "Vendedor" &&
                rol != "Cartera")
            {
                return RedirectToAction("Login", "Auto");
            }

            // =====================================================
            // FILTRO POR DEFECTO
            // =====================================================
            filtro ??= new VentaHistorialFiltroVM();

            // =====================================================
            // OBTENER HISTORIAL
            // =====================================================
            var resultado = await _ventasService.ObtenerVentasAsync(filtro);

            // =====================================================
            // VIEWMODEL
            // =====================================================
            var vm = new HistorialVentasIndexVM
            {
                Filtro = filtro,

                Ventas = resultado.Ventas,

                TotalVentas = resultado.TotalRegistros,

                TotalPagadas = resultado.TotalPagadas,

                TotalAbonadas = resultado.TotalAbonadas,

                TotalSaldoPendiente = resultado.TotalSaldoPendiente,

                PaginaActual = resultado.PaginaActual,

                TotalPaginas = resultado.TotalPaginas,

                TotalRegistros = resultado.TotalRegistros,

                RegistrosPorPagina = filtro.RegistrosPorPagina
            };

            return View("Ventas", vm);
        }
        public async Task<IActionResult> Detalle(int id)
        {
            var vm = await _ventasService.ObtenerDetalleVentaAsync(id);

            if (vm == null)
                return NotFound();

            return View(vm);
        }

        // =========================================================
        // 🖨 IMPRIMIR UNA VENTA
        // =========================================================
        public async Task<IActionResult> ImprimirVenta(int id)
        {
            var vm = await _ventasService.ObtenerDetalleVentaAsync(id);

            if (vm == null)
                return NotFound();

            return View("Reporte", vm);
        }

        // =========================================================
        // 📄 EXPORTAR VENTA A PDF
        // =========================================================
        public async Task<IActionResult> ExportarPdfVenta(int id)
        {
            var vm = await _ventasService.ObtenerDetalleVentaAsync(id);

            if (vm == null)
                return NotFound();

            var documento = new VentaPdfDocument(vm);

            var pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Venta_{vm.ID_Pedido}.pdf");
        }

        // =========================================================
        // 📧 ENVIAR REPORTE POR CORREO
        // =========================================================
        public async Task<IActionResult> EnviarCorreoVenta(int id)
        {
            var vm = await _ventasService.ObtenerDetalleVentaAsync(id);

            if (vm == null)
                return NotFound();
            // =====================================================
            // VALIDAR CORREO DEL CLIENTE
            // =====================================================

            if (string.IsNullOrWhiteSpace(vm.CorreoCliente))
            {
                TempData["Error"] =
                    "El cliente no tiene un correo electrónico registrado.";

                return RedirectToAction(nameof(Detalle), new { id });
            }


            // =====================================================
            // USUARIO QUE REALIZA EL ENVÍO
            // =====================================================

            string usuario = HttpContext.Session.GetString("UsuarioNombre")
                             ?? "Usuario no identificado";

            try
            {
                // =====================================================
                // GENERAR PDF EN MEMORIA
                // =====================================================

                var documento = new VentaPdfDocument(vm);

                var pdf = documento.GeneratePdf();


                // =====================================================
                // DATOS DEL CORREO
                // =====================================================

                string asunto =
                    $"ERP INVENTARIOWEB | Reporte de Venta {vm.NumeroFactura} - Pedido {vm.ID_Pedido}";

                string mensaje = $@"Estimado(a) {vm.Cliente}:

Reciba un cordial saludo.

Adjuntamos el reporte correspondiente a su venta registrada en ERP INVENTARIOWEB.

Resumen de la venta

• Pedido: {vm.ID_Pedido}
• Factura: {vm.NumeroFactura}
• Fecha de la venta: {vm.Fecha:dd/MM/yyyy}
• Fecha del reporte: {vm.FechaReporte:dd/MM/yyyy HH:mm}
• Total de la venta: {vm.TotalVenta:C0}
• Estado del pago: {vm.EstadoPago}
• Estado del despacho: {vm.EstadoDespacho}
• Tipo de despacho: {vm.TipoDespacho}

El documento PDF adjunto contiene el detalle completo de la transacción, incluyendo:

• Información general de la venta.
• Productos adquiridos.
• Resumen financiero.
• Historial de pagos registrados.
• Estado comercial actualizado.

Este documento corresponde a la información registrada en el sistema al momento de su generación y tiene carácter informativo.

Agradecemos su confianza en nuestros productos.

Cordialmente,

ERP INVENTARIOWEB
Sistema Integrado de Inventario, Producción, Ventas y Cartera.

Este es un correo generado automáticamente.
Por favor, no responda este mensaje.";

                string nombreArchivo =
                    $"Venta_{vm.ID_Pedido}.pdf";

                // =====================================================
                // ENVIAR CORREO
                // =====================================================
                await _emailService.EnviarCorreoAsync(
                    vm.CorreoCliente,
                    asunto,
                    mensaje,
                    pdf,
                    nombreArchivo
                );

                await _correoEnviadoService.RegistrarAsync(
                    vm.ID_Pedido,
                    vm.CorreoCliente,
                    usuario,
                    "ENVIADO",
                    "Correo enviado correctamente."
                );




                TempData["Info"] =
                    $"Reporte enviado correctamente al correo: {vm.CorreoCliente}";
            }
            catch (Exception ex)
            {
                await _correoEnviadoService.RegistrarAsync(
                    vm.ID_Pedido,
                    vm.CorreoCliente,
                    usuario,
                    "ERROR",
                    ex.Message
                );

                TempData["Error"] =
                    $"No fue posible enviar el correo. Error: {ex.Message}";
            }


            return RedirectToAction(nameof(Detalle), new { id });
        }

        // =========================================================
        // 📊 EXPORTAR HISTORIAL DE VENTAS A EXCEL
        // Exporta todas las ventas según los filtros aplicados.
        // No utiliza paginación.
        // =========================================================
        public async Task<IActionResult> ExportarExcel(VentaHistorialFiltroVM filtro)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? "";

            if (rol != "Administrador" &&
                rol != "Vendedor" &&
                rol != "Cartera")
            {
                return RedirectToAction("Login", "Auto");
            }

            var ventas = await _ventasService.ObtenerVentasExportacionAsync(filtro);

            using var libro = new XLWorkbook();

            var hoja = libro.Worksheets.Add("Historial Ventas");

            // =====================================================
            // ENCABEZADO DEL REPORTE
            // =====================================================

            hoja.Cell("A1").Value = "ERP INVENTARIOWEB";
            hoja.Cell("A2").Value = "Módulo de Ventas";
            hoja.Cell("A3").Value = "Reporte Historial General de Ventas";

            hoja.Cell("J1").Value = "Fecha:";
            hoja.Cell("K1").Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            hoja.Cell("J2").Value = "Total registros:";
            hoja.Cell("K2").Value = ventas.Count;

            hoja.Range("A1:A3").Style.Font.Bold = true;
            hoja.Range("A1:A3").Style.Font.FontSize = 13;

            hoja.Range("J1:K2").Style.Font.Bold = true;
                        
            // =====================================================
            // ENCABEZADOS DE LA TABLA
            // =====================================================
            hoja.Cell(5, 1).Value = "Factura";
            hoja.Cell(5, 2).Value = "Pedido";
            hoja.Cell(5, 3).Value = "Fecha";
            hoja.Cell(5, 4).Value = "Cliente";
            hoja.Cell(5, 5).Value = "Tipo Venta";
            hoja.Cell(5, 6).Value = "Estado Pago";
            hoja.Cell(5, 7).Value = "Estado Despacho";
            hoja.Cell(5, 8).Value = "Total";
            hoja.Cell(5, 9).Value = "Abonado";
            hoja.Cell(5, 10).Value = "Saldo";
            hoja.Cell(5, 11).Value = "Productos";
            hoja.Cell(5, 12).Value = "Unidades";

            hoja.Range(5, 1, 5, 12).Style.Font.Bold = true;
            hoja.Range(5, 1, 5, 12).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            hoja.Range(5, 1, 5, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // =====================================================
            // DETALLE
            // =====================================================

            int fila = 6;

            foreach (var item in ventas)
            {
                hoja.Cell(fila, 1).Value =
                 item.ID_Despacho > 0
                    ? $"FAC-{item.ID_Despacho}"
                    : "SIN FACTURA";

                hoja.Cell(fila, 2).Value = item.ID_Pedido;
                hoja.Cell(fila, 3).Value = item.Fecha;
                hoja.Cell(fila, 4).Value = item.Cliente;
                hoja.Cell(fila, 5).Value = item.TipoVenta;
                hoja.Cell(fila, 6).Value = item.EstadoPago;
                hoja.Cell(fila, 7).Value = item.EstadoDespacho;
                hoja.Cell(fila, 8).Value = item.TotalVenta;
                hoja.Cell(fila, 9).Value = item.TotalAbonado;
                hoja.Cell(fila, 10).Value = item.Saldo;
                hoja.Cell(fila, 11).Value = item.TotalProductos;
                hoja.Cell(fila, 12).Value = item.TotalUnidades;

                fila++;
            }
            // Última fila de la tabla
            int ultimaFilaTabla = fila - 1;

            // =====================================================
            // RESUMEN DEL REPORTE
            // =====================================================

            fila += 2;

            // Encabezado del resumen
            hoja.Cell(fila, 1).Value = "RESUMEN EJECUTIVO";

            hoja.Range(fila, 1, fila, 2).Style.Fill.BackgroundColor =
                XLColor.LightSteelBlue;

            hoja.Range(fila, 1, fila, 2).Style.Font.Bold = true;

            hoja.Range(fila, 1, fila, 2).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Range(fila, 1, fila, 2).Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            fila++;

            // Total registros
            hoja.Cell(fila, 1).Value = "Total registros";
            hoja.Cell(fila, 2).Value = ventas.Count;

            fila++;

            // Ventas pagadas
            hoja.Cell(fila, 1).Value = "Ventas pagadas";
            hoja.Cell(fila, 2).Value =
                ventas.Count(x => x.EstadoPago == "PAGADO");

            fila++;

            // Ventas abonadas
            hoja.Cell(fila, 1).Value = "Ventas abonadas";
            hoja.Cell(fila, 2).Value =
                ventas.Count(x => x.EstadoPago == "ABONADO");

            fila++;

            // Valor vendido
            hoja.Cell(fila, 1).Value = "Valor vendido";
            hoja.Cell(fila, 2).Value =
                ventas.Sum(x => x.TotalVenta);
            hoja.Cell(fila, 2).Style.NumberFormat.Format = "#,##0";

            fila++;

            // Valor abonado
            hoja.Cell(fila, 1).Value = "Valor abonado";
            hoja.Cell(fila, 2).Value =
                ventas.Sum(x => x.TotalAbonado);
            hoja.Cell(fila, 2).Style.NumberFormat.Format = "#,##0";

            fila++;

            // Saldo pendiente
            hoja.Cell(fila, 1).Value = "Saldo pendiente";
            hoja.Cell(fila, 2).Value =
                ventas.Sum(x => x.Saldo);
            hoja.Cell(fila, 2).Style.NumberFormat.Format = "#,##0";


            // =====================================================
            // TOTAL GENERAL
            // =====================================================

            hoja.Cell(ultimaFilaTabla + 1, 7).Value = "TOTAL SALDO PENDIENTE";

            hoja.Cell(ultimaFilaTabla + 1, 10).Value =
                ventas.Sum(x => x.Saldo);

            var rangoTotal = hoja.Range(
                ultimaFilaTabla + 1, 7,
                ultimaFilaTabla + 1, 10);

            rangoTotal.Style.Font.Bold = true;
            rangoTotal.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Línea de separación superior e inferior
            rangoTotal.Style.Border.TopBorder = XLBorderStyleValues.Thick;
            rangoTotal.Style.Border.BottomBorder = XLBorderStyleValues.Thick;

            // =====================================================
            // FORMATOS
            // =====================================================

            // Fecha
            hoja.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";

            // Valores monetarios
            hoja.Column(8).Style.NumberFormat.Format = "#,##0";   // Total
            hoja.Column(9).Style.NumberFormat.Format = "#,##0";   // Abonado
            hoja.Column(10).Style.NumberFormat.Format = "#,##0";  // Saldo
                                                                  // Ajustar ancho de columnas
            hoja.Columns().AdjustToContents();

            // =====================================================
            // ESTILO DE LA TABLA
            // =====================================================

            // Bordes
            hoja.Range(5, 1, ultimaFilaTabla, 12).Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            hoja.Range(5, 1, ultimaFilaTabla, 12).Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            // Encabezados
            hoja.Range(5, 1, 5, 12).Style.Font.Bold = true;
            hoja.Range(5, 1, 5, 12).Style.Fill.BackgroundColor =
                XLColor.LightSteelBlue;

            // Alinear encabezados
            hoja.Range(5, 1, 5, 12).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // Centrar columnas de identificación
            hoja.Column(1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Column(2).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Column(3).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Column(5).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Column(6).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Column(7).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Column(11).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Column(12).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            // Congelar encabezados
            hoja.SheetView.FreezeRows(5);

            using var stream = new MemoryStream();
            hoja.Range(5, 1, ultimaFilaTabla, 12)
            .SetAutoFilter();

            libro.SaveAs(stream);

            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HistorialVentas_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        // =========================================================
        // 🖨 IMPRIMIR HISTORIAL DE VENTAS
        // Muestra una vista lista para impresión con todos los
        // registros que cumplen los filtros aplicados.
        // =========================================================
        public async Task<IActionResult> Imprimir(VentaHistorialFiltroVM filtro)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? "";

            if (rol != "Administrador" &&
                rol != "Vendedor" &&
                rol != "Cartera")
            {
                return RedirectToAction("Login", "Auto");
            }

            var ventas = await _ventasService.ObtenerVentasExportacionAsync(filtro);

            return View("Imprimir", ventas);
        }

        // =========================================================
        // 📄 EXPORTAR HISTORIAL DE VENTAS A PDF
        // Por ahora reutiliza la vista de impresión.
        // Posteriormente se reemplazará por un generador de PDF.
        // =========================================================
        public async Task<IActionResult> ExportarPdf(VentaHistorialFiltroVM filtro)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? "";

            if (rol != "Administrador" &&
                rol != "Vendedor" &&
                rol != "Cartera")
            {
                return RedirectToAction("Login", "Auto");
            }

            var ventas = await _ventasService.ObtenerVentasExportacionAsync(filtro);

            return View("Imprimir", ventas);
        }

    }
}
