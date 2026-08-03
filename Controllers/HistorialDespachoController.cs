using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Filters;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Borders;
using iText.Layout.Properties;
using InventarioWEB.Models;
using InventarioWEB.Services;
using InventarioWEB.Constants;


namespace InventarioWEB.Controllers
{
    [ValidarSesion]
    public class HistorialDespachoController : Controller
    {        
        private readonly MovimientoVentasDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly FacturaPdfService _facturaPdfService;

        public HistorialDespachoController(
             MovimientoVentasDbContext context,
             IWebHostEnvironment env,
             IEmailService emailService,
             FacturaPdfService facturaPdfService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
            _facturaPdfService = facturaPdfService;
        }
        private bool TieneAcceso()
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            return rol == "Administrador"
                || rol == "Producción";
        }

        //==========================================================
        // HISTORIAL DE DESPACHOS
        //==========================================================
        public async Task<IActionResult> Index()
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            var despachos = await _context.Despachos
                 .Include(d => d.Pedido)
                 .ThenInclude(p => p.Cliente)
                 .OrderBy(d => d.ID_Pedido)
                 .ThenBy(d => d.ID_Despacho)
                 .AsNoTracking()
                 .ToListAsync();
            return View(despachos);

            //return View("~/Views/Historial/Index.cshtml", despachos);
        }

        //==========================================================
        // DETALLE DEL DESPACHO
        //==========================================================
        public async Task<IActionResult> Detalle(int id)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            var despacho = await _context.Despachos
                .Include(d => d.Pedido)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Talla)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Referencia)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ID_Despacho == id);

            if (despacho == null)
                return NotFound();
            return View(despacho);
            //return View("~/Views/Historial/Detalle.cshtml", despacho);

        }

       
        public async Task<IActionResult> Factura(int id)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");
                        
            var pdf = await _facturaPdfService.GenerarFacturaPdfAsync(id);

            return File(
                pdf,
                "application/pdf",
                $"Factura_{id}.pdf");
        }

        // ==========================================================
        // 📧 ENVIAR FACTURA POR CORREO
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarFacturaCorreo(int id)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            try
            {
                // ======================================================
                // OBTENER DESPACHO
                // ======================================================

                var despacho = await _context.Despachos
                    .Include(d => d.Pedido)
                        .ThenInclude(p => p.Cliente)
                    .FirstOrDefaultAsync(d => d.ID_Despacho == id);

                if (despacho == null)
                {
                    TempData["Error"] = "No se encontró el despacho.";
                    return RedirectToAction(nameof(Index));
                }

                // ======================================================
                // VALIDAR CORREO DEL CLIENTE
                // ======================================================

                // ======================================================
                // VALIDAR PEDIDO Y CLIENTE
                // ======================================================

                if (despacho.Pedido == null)
                {
                    TempData["Error"] =
                        "El despacho no tiene un pedido asociado.";

                    return RedirectToAction(nameof(Index));
                }

                var cliente = despacho.Pedido.Cliente;

                if (cliente == null)
                {
                    TempData["Error"] =
                        "El pedido no tiene un cliente asociado.";

                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrWhiteSpace(cliente.Correo))
                {
                    TempData["Error"] =
                        "El cliente no tiene un correo electrónico registrado.";

                    return RedirectToAction(nameof(Index));
                }

                // ======================================================
                // AQUÍ SE GENERARÁ EL PDF
                // ======================================================

                // ======================================================
                // GENERAR PDF
                // ======================================================

                var pdf = await _facturaPdfService.GenerarFacturaPdfAsync(id);

                // ======================================================
                // ENVIAR CORREO
                // ======================================================

                await _emailService.EnviarCorreoAsync(
                    destinatario: cliente.Correo,
                    asunto: $"Factura No. {despacho.ID_Despacho} - {ReportesConstantes.Empresa}",
                    mensaje:
                        $"Estimado(a) {cliente.Nombre} {cliente.Apellido},\n\n" +
                        $"Adjuntamos la factura correspondiente al despacho No. {despacho.ID_Despacho}.\n\n" +
                        $"Gracias por confiar en {ReportesConstantes.Empresa}.\n\n" +
                        $"Este mensaje fue generado automáticamente por el ERP InventarioWEB.",
                    archivoAdjunto: pdf,
                    nombreArchivo: $"Factura_{despacho.ID_Despacho}.pdf"
                );

                // ======================================================
                // REGISTRAR TRAZABILIDAD DEL ENVÍO
                // ======================================================

                despacho.CorreoEnviado = true;

                despacho.FechaEnvioCorreo = DateTime.Now;

                despacho.CorreoDestino = cliente.Correo;

                despacho.UsuarioEnvioCorreo =
                    HttpContext.Session.GetString("UsuarioNombre") ?? "Sistema";

                await _context.SaveChangesAsync();

                // ======================================================
                // MENSAJE
                // ======================================================

                TempData["Success"] =
                    $"La factura fue enviada correctamente a {cliente.Correo}.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
