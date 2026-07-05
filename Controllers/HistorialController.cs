using ClosedXML.Excel;
using InventarioWEB.Data;
using InventarioWEB.DTOs;
using InventarioWEB.Filters;
using InventarioWEB.Models;
using InventarioWEB.Services;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace InventarioWEB.Controllers
{
    [ValidarSesion]
    public class HistorialController : Controller
    {
        private readonly MovimientoVentasDbContext _context;
        private readonly HistorialInventarioService _historialService;

        public HistorialController(
            MovimientoVentasDbContext context,
            HistorialInventarioService historialService)
        {
            _context = context;
            _historialService = historialService;
        }

        // ==============================
        // 🛒 VENTAS (REDIRECCIÓN CONTROLADA)
        // ==============================
        public IActionResult Ventas()
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" && rol != "Vendedor" && rol != "Cartera")
                return RedirectToAction("Login", "Auto");

            return RedirectToAction("Index", "HistorialVentas");
        }

        // ==============================
        // 🚚 DESPACHOS
        // ==============================
        public async Task<IActionResult> Despachos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" && rol != "Producción")
                return RedirectToAction("Login", "Auto");

            var query = _context.Despachos.AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(d => d.Fecha >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(d => d.Fecha <= fechaFin.Value);

            var resultado = await query
                .OrderByDescending(d => d.Fecha)
                .ToListAsync();

            return View(resultado ?? new List<Despacho>());
        }




        // =========================================================
        // 🔧 BUILDER CENTRAL DE FILTRO (REUTILIZABLE ERP)
        // =========================================================
        private KardexFilterDto BuildFilter(
            int? idProducto,
            string? referencia,
            string? color,
            string? tela,
            string? talla,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            return new KardexFilterDto
            {
                IdProducto = idProducto,
                Referencia = referencia,
                Color = color,
                Tela = tela,
                Talla = talla,
                Desde = fechaInicio,
                Hasta = fechaFin
            };
        }

        // =========================================================
        // 📊 KARDEX (HISTORIAL CENTRAL)
        // =========================================================
        public async Task<IActionResult> Kardex(
            int? idProducto,
            string? referencia,
            string? color,
            string? tela,
            string? talla,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" && rol != "Producción" && rol != "Vendedor")
                return RedirectToAction("Login", "Auto");

            // =========================================================
            // 🔥 DTO FILTRO (CENTRALIZADO - SIN DUPLICACIÓN)
            // =========================================================
            var filter = BuildFilter(
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin
            );

            // =========================================================
            // 🔥 SERVICIO (FUENTE ÚNICA DE VERDAD)
            // =========================================================
            var resultado = await _historialService.ObtenerKardexCompletoAsync(filter);

            // =========================================================
            // 🔥 DATOS PARA UI (FILTROS)
            // =========================================================
            ViewBag.Generos = await _context.Generos
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Referencias = await _context.Referencias
                .AsNoTracking()
                .ToListAsync();

            // =========================================================
            // 🔥 ESTADO DE FILTROS (UI STATE)
            // =========================================================
            ViewBag.Filtros = new
            {
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin
            };

            // =========================================================
            // 📊 DATOS PARA VISTA
            // =========================================================
            ViewBag.Grafica = resultado.Grafica;
            ViewBag.TotalEntradas = resultado.TotalEntradas;
            ViewBag.TotalSalidas = resultado.TotalSalidas;

            return View(resultado.Movimientos);
        }

        public async Task<IActionResult> KardexProducto(
            int idProducto,
            string? referencia,
            string? color,
            string? tela,
            string? talla,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" &&
                rol != "Producción" &&
                rol != "Vendedor")
            {
                return RedirectToAction("Login", "Auto");
            }

            if (idProducto <= 0)
                return BadRequest("Producto inválido");

            // =========================================================
            // 🔥 BUILDER CENTRALIZADO
            // =========================================================
            var filter = BuildFilter(
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin
            );

            // =========================================================
            // 🔥 SERVICIO (FUENTE ÚNICA DE VERDAD)
            // =========================================================
            var resultado = await _historialService.ObtenerKardexCompletoAsync(filter);

            // =========================================================
            // 📊 VIEWDATA PARA UI
            // =========================================================
            ViewBag.Grafica = resultado.Grafica;
            ViewBag.TotalEntradas = resultado.TotalEntradas;
            ViewBag.TotalSalidas = resultado.TotalSalidas;
            ViewBag.IdProducto = idProducto;

            ViewBag.Filtros = new
            {
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin
            };

            return View(resultado.Movimientos);
        }


        /*
        // ==============================
        // 📊 KARDEX ANALÍTICO
        // ==============================
        public async Task<IActionResult> KardexAnalitico(
            int? idProducto,
            string? referencia,
            string? color,
            string? tela,
            string? talla,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" &&
                rol != "Producción" &&
                rol != "Vendedor")
            {
                return RedirectToAction("Login", "Auto");
            }

            // ==============================
            // FILTRO CENTRALIZADO
            // ==============================
            var filter = BuildFilter(
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin);

            // ==============================
            // SERVICIO
            // ==============================
            var datos = await _historialService.ObtenerKardexAnaliticoAsync(filter);

            // ==============================
            // DATOS PARA FILTROS
            // ==============================
            ViewBag.Generos = await _context.Generos
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Referencias = await _context.Referencias
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Filtros = new
            {
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin
            };

            return View(datos);
        }
        */
        // ==================ALERTAS DE STOCK======================================
        public async Task<IActionResult> AlertasStock()
        {
            var alertas = await _historialService.ObtenerStockMinimoAsync();
            return View(alertas);
        }

        // =========================================================
        // 📊 GRÁFICA DINÁMICA KARDEX (DRILL-DOWN)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GraficaKardex(
            int? idProducto,
            string? referencia,
            string? color,
            string? tela,
            string? talla,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            // 🔥 CAMBIO: uso del Builder centralizado
            var filter = BuildFilter(
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin
            );

            var resultado = await _historialService.ObtenerKardexCompletoAsync(filter);

            return Json(new
            {
                success = true,
                data = resultado.Grafica
            });
        }


        // =========================================================
        // 📦 EXPORTAR EXCEL
        // =========================================================
        public async Task<IActionResult> ExportarExcel(
            int? idProducto,
            string? referencia,
            string? color,
            string? tela,
            string? talla,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            // 🔥 CAMBIO: uso del Builder centralizado
            var filter = BuildFilter(
                idProducto,
                referencia,
                color,
                tela,
                talla,
                fechaInicio,
                fechaFin
            );

            var resultado = await _historialService.ObtenerKardexCompletoAsync(filter);

            var datos = resultado.Movimientos;

            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("Kardex");

            hoja.Cell(1, 1).Value = "REPORTE KARDEX INVENTARIO";
            hoja.Cell(3, 1).Value = "Fecha";
            hoja.Cell(3, 2).Value = "Movimiento";
            hoja.Cell(3, 3).Value = "Referencia";
            hoja.Cell(3, 4).Value = "Entrada";
            hoja.Cell(3, 5).Value = "Salida";
            hoja.Cell(3, 6).Value = "Saldo";
            hoja.Cell(3, 7).Value = "Usuario";

            for (int i = 0; i < datos.Count; i++)
            {
                var item = datos[i];
                var fila = i + 4;

                hoja.Cell(fila, 1).Value = item.Fecha;
                hoja.Cell(fila, 2).Value = item.TipoMovimiento;
                hoja.Cell(fila, 3).Value = item.Referencia;

                hoja.Cell(fila, 4).Value = item.EntradaStock;
                hoja.Cell(fila, 5).Value = item.SalidaStock;

                hoja.Cell(fila, 6).Value = item.StockActual;
                hoja.Cell(fila, 7).Value = item.UsuarioNombre;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Kardex.xlsx"
            );
        }

    }
}