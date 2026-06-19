using Microsoft.AspNetCore.Mvc;
using InventarioWEB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // 🔥 necesario para Session
using ClosedXML.Excel;
using System.IO;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;

namespace InventarioWEB.Controllers
{
    public class HistorialController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        public HistorialController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ==============================
        // 🛒 HISTORIAL DE VENTAS (PEDIDOS)
        // ==============================
        public async Task<IActionResult> Ventas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" && rol != "Vendedor" && rol != "Cartera")
                return RedirectToAction("Login", "Auto");

            var query = _context.Pedidos
                //.Include(p => p.Cliente) // 🔸 activar solo si existe navegación
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(p => p.Fecha >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(p => p.Fecha <= fechaFin.Value);

            var resultado = await query
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

            return View(resultado ?? new List<Pedido>());

            // return View(resultado ?? new List<HistorialInventario>());
            //new List<Pedido>());


        }

        // ==============================
        // 🚚 HISTORIAL DE DESPACHOS
        // ==============================
        public async Task<IActionResult> Despachos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;
           

            if (rol != "Administrador" && rol != "Producción")
                return RedirectToAction("Login", "Auto");

            var query = _context.Despachos
                //.Include(d => d.Pedido) // 🔸 activar solo si existe navegación
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(d => d.Fecha >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(d => d.Fecha <= fechaFin.Value);

            var resultado = await query
                .OrderByDescending(d => d.Fecha)
                .ToListAsync();

            return View(resultado ?? new List<Despacho>());
           

          //  return View(resultado);
        }

        public async Task<IActionResult> Kardex(
             int? genero,
             int? referencia,
             string? sku,
             string? color,
             string? tela,
             string? talla,
             DateTime? fechaInicio,
             DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" && rol != "Producción" && rol != "Vendedor")
                return RedirectToAction("Login", "Auto");

            var query = _context.HistorialInventario.AsQueryable();

            
            // =====================================
            // COMBOS FILTROS
            // =====================================

            ViewBag.Generos = await _context.Generos
                .OrderBy(g => g.DescripGenero)
                .ToListAsync();

            ViewBag.GeneroSeleccionado = genero;
            ViewBag.DebugGenero = genero;// Temporal para verificar valor           

            ViewBag.Referencias = genero.HasValue
                ? await _context.Referencias
                    .Where(r => r.ID_Genero == genero.Value && r.Activo)
                    .OrderBy(r => r.DescripReferencia)
                    .ToListAsync()
                : new List<Referencia>();

            var referenciasDebug = genero.HasValue
                ? await _context.Referencias
                    .Where(r => r.ID_Genero == genero.Value && r.Activo)
                    .ToListAsync()
                : new List<Referencia>();

            ViewBag.DebugReferencias = referenciasDebug.Count;

            ViewBag.ReferenciaSeleccionada = referencia;
              
            // =====================================
            // OBTENER NOMBRE DE REFERENCIA
            // =====================================

            string? nombreReferencia = null;

            if (referencia.HasValue)
            {
                nombreReferencia = await _context.Referencias
                    .Where(r => r.ID_Referencias == referencia.Value)
                    .Select(r => r.DescripReferencia)
                    .FirstOrDefaultAsync();
            }

            // =====================================
            // NORMALIZAR INPUTS
            // =====================================

            sku = sku?.Trim();
            color = color?.Trim();
            tela = tela?.Trim();
            talla = talla?.Trim();

            // =====================================
            // FILTROS
            // =====================================

            if (!string.IsNullOrWhiteSpace(sku))
            {
                query = query.Where(h =>
                    h.SkuArticulo != null &&
                    h.SkuArticulo.Contains(sku));
            }

            if (!string.IsNullOrWhiteSpace(nombreReferencia))
            {
                query = query.Where(h =>
                    h.Referencia == nombreReferencia);
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(h =>
                    h.Color != null &&
                    h.Color.Contains(color));
            }

            if (!string.IsNullOrWhiteSpace(tela))
            {
                query = query.Where(h =>
                    h.Tela != null &&
                    h.Tela.Contains(tela));
            }

            if (!string.IsNullOrWhiteSpace(talla))
            {
                query = query.Where(h =>
                    h.Talla != null &&
                    h.Talla.Contains(talla));
            }

            // =====================================
            // FECHAS
            // =====================================

            if (fechaInicio.HasValue)
            {
                query = query.Where(h =>
                    h.FechaRegistro >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(h =>
                    h.FechaRegistro <= fin);
            }

            // =====================================
            // TRAER DATOS
            // =====================================

            var lista = await query
                .OrderBy(h => h.FechaRegistro)
                .ThenBy(h => h.Id)
                .ToListAsync();
            // ✅ SALDO ACUMULADO
            int saldo = 0;

            var kardex = lista.Select(h =>
            {
                int entrada = 0;
                int salida = 0;

                // 🎯 Clasificación de movimientos
                if (h.TipoMovimiento == "PRODUCCION" ||
                    h.TipoMovimiento == "COMPRA" ||
                    h.TipoMovimiento == "AJUSTE")
                {
                    entrada = h.Cantidad;
                }
                else if (h.TipoMovimiento == "VENTA_DESPACHO")
                {
                    salida = Math.Abs(h.Cantidad);
                }

                saldo += entrada - salida;

                return new KardexViewModel
                {
                    Fecha = h.FechaRegistro,
                    TipoMovimiento = h.TipoMovimiento,
                    Sku = h.SkuArticulo,
                    Referencia = h.Referencia,
                    Color = h.Color,
                    Talla = h.Talla,
                    Tela = h.Tela,
                    Documento = h.DocumentoReferencia,
                    Usuario = h.UsuarioNombre,


                    Entrada = entrada,
                    Salida = salida,
                    Saldo = saldo
                };
            }).ToList();


            var grafica = kardex
                .GroupBy(x => x.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Entrada = g.Sum(x => x.Entrada),
                    Salida = g.Sum(x => x.Salida)
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            ViewBag.Grafica = grafica;
            // ✅ RETORNAR KÁRDEX REAL
            return View(kardex);
        }
        // 📄 Exportar Kardex a Excel
        public async Task<IActionResult> ExportarExcel(
            string? sku,
            string? referencia,
            string? color,
            string? talla,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var rol = HttpContext.Session.GetString("Rol")?.Trim() ?? string.Empty;

            if (rol != "Administrador" && rol != "Producción" && rol != "Vendedor")
                return RedirectToAction("Login", "Auto");

            var query = _context.HistorialInventario.AsQueryable();

            if (!string.IsNullOrWhiteSpace(sku))
                query = query.Where(h => h.SkuArticulo.Contains(sku));

            if (!string.IsNullOrWhiteSpace(referencia))
                query = query.Where(h => h.Referencia.Contains(referencia.Trim()));

            if (!string.IsNullOrWhiteSpace(color))
                query = query.Where(h => h.Color.Contains(color.Trim()));

            if (!string.IsNullOrWhiteSpace(talla))
                query = query.Where(h => h.Talla.Contains(talla.Trim()));



            if (fechaInicio.HasValue)
                query = query.Where(h => h.FechaRegistro >= fechaInicio.Value);

            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(h => h.FechaRegistro <= fin);
            }

            /*

            if (fechaInicio.HasValue)
                query = query.Where(h => h.FechaRegistro >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(h => h.FechaRegistro <= fechaFin.Value);
            */

            var datos = await query
            .OrderBy(h => h.FechaRegistro)
            .ThenBy(h => h.Id) // o tu PK
            .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var hoja = workbook.Worksheets.Add("Kardex");

                // =====================================
                // TÍTULO
                // =====================================

                hoja.Range("A1:F1").Merge();

                hoja.Cell("A1").Value = "REPORTE KARDEX INVENTARIO";

                hoja.Cell("A1").Style.Font.Bold = true;
                hoja.Cell("A1").Style.Font.FontSize = 16;

                hoja.Cell("A1").Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                // =====================================
                // ENCABEZADOS
                // =====================================

                hoja.Cell(3, 1).Value = "Fecha";
                hoja.Cell(3, 2).Value = "SKU";
                hoja.Cell(3, 3).Value = "Tipo";
                hoja.Cell(3, 4).Value = "Cantidad";
                hoja.Cell(3, 5).Value = "Stock";
                hoja.Cell(3, 6).Value = "Usuario";

                var encabezado = hoja.Range("A3:F3");

                encabezado.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                encabezado.Style.Font.FontColor = XLColor.White;
                encabezado.Style.Font.Bold = true;

                encabezado.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                // =====================================
                // DATOS
                // =====================================

                for (int i = 0; i < datos.Count; i++)
                {
                    var fila = i + 4;

                    hoja.Cell(fila, 1).Value =
                        datos[i].FechaRegistro;

                    hoja.Cell(fila, 1).Style.DateFormat.Format =
                        "yyyy-MM-dd HH:mm";

                    hoja.Cell(fila, 2).Value =
                        datos[i].SkuArticulo ?? "";

                    hoja.Cell(fila, 3).Value =
                        datos[i].TipoMovimiento ?? "";

                    hoja.Cell(fila, 4).Value =
                        datos[i].Cantidad;

                    hoja.Cell(fila, 5).Value =
                        datos[i].StockActual;

                    hoja.Cell(fila, 6).Value =
                        datos[i].UsuarioNombre ?? "";
                }

                // =====================================
                // TOTAL
                // =====================================

                int filaTotal = datos.Count + 5;

                hoja.Cell(filaTotal, 3).Value = "TOTAL";

                hoja.Cell(filaTotal, 4).FormulaA1 =
                    $"SUM(D4:D{filaTotal - 1})";

                hoja.Range($"C{filaTotal}:D{filaTotal}")
                    .Style.Font.Bold = true;

                hoja.Range($"C{filaTotal}:D{filaTotal}")
                    .Style.Fill.BackgroundColor = XLColor.LightGray;

                // =====================================
                // FORMATO
                // =====================================

                var rango = hoja.RangeUsed();

                if (rango != null)
                {
                    rango.Style.Border.OutsideBorder =
                        XLBorderStyleValues.Thin;

                    rango.Style.Border.InsideBorder =
                        XLBorderStyleValues.Thin;

                    rango.SetAutoFilter();
                }

                hoja.SheetView.FreezeRows(3);

                hoja.Columns().AdjustToContents();

                // =====================================
                // EXPORTAR
                // =====================================

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Kardex.xlsx");
                }
            }
        }
        
        // =====================================
        // AJAX FILTROS KARDEX
        // =====================================

        [HttpGet]
        public async Task<IActionResult> ObtenerGeneros()
        {
            var data = await _context.Generos
                .OrderBy(g => g.DescripGenero)
                .Select(g => new
                {
                    value = g.ID_Genero,
                    text = g.DescripGenero
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerReferenciasPorGenero(int idGenero)
        {
            var data = await _context.Referencias
                .Where(r => r.ID_Genero == idGenero && r.Activo)
                .OrderBy(r => r.DescripReferencia)
                .Select(r => new
                {
                    value = r.ID_Referencias,
                    text = r.DescripReferencia
                })
                .ToListAsync();

            return Json(data);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerTallasPorGenero(int idGenero)
        {
            var data = await _context.Tallas
            .Where(t => t.ID_Genero == idGenero && t.Activo)
            .OrderBy(t => t.DescripTalla)
            .Select(t => new
            {
                value = t.ID_Tallas,
                text = t.DescripTalla
            })
            .ToListAsync();
                return Json(data);
         }


        [HttpGet]
        public async Task<IActionResult> ObtenerTelas()
        {
            var data = await _context.Telas
                .Where(t => t.Activo)
                .OrderBy(t => t.DescripTela)
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
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    value = c.ID_Color,
                    text = c.Nombre
                })
                .ToListAsync();

            return Json(data);
        }

    }
}
