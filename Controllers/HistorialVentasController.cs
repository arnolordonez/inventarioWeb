using InventarioWEB.Data;
using InventarioWEB.Filters;
using InventarioWEB.Services;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioWEB.Controllers
{
    [ValidarSesion]
    public class HistorialVentasController : Controller
    {
        private readonly HistorialVentasService _ventasService;
        private readonly MovimientoVentasDbContext _context;

        public HistorialVentasController(
            HistorialVentasService ventasService,
            MovimientoVentasDbContext context)
        {
            _ventasService = ventasService;
            _context = context;
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
        public async Task<IActionResult> Reporte(int id)
        {
            // Reutilizamos exactamente la misma fuente de verdad del detalle
            var vm = await _ventasService.ObtenerDetalleVentaAsync(id);

            if (vm == null)
                return NotFound();

            // Vista especializada tipo documento comercial (no logística)
            return View("Reporte", vm);
        }
    }
}
