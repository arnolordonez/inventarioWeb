
using InventarioWEB.Data;
using InventarioWEB.DTOs;
using InventarioWEB.Filters;
using InventarioWEB.Models;
using InventarioWEB.Services;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Constants;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using ClosedXML.Excel;
//using ClosedXML.Graphics;


namespace InventarioWEB.Controllers
{
    [ValidarSesion]
    public class HistorialKardexController : Controller
    {
        private readonly MovimientoVentasDbContext _context;
        private readonly HistorialInventarioService _historialService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HistorialKardexController(
            MovimientoVentasDbContext context,
            HistorialInventarioService historialService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _historialService = historialService;
            _webHostEnvironment = webHostEnvironment;
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
            int? idGenero,
            int? idReferencia,
            int? idTalla,
            int? idTela,
            int? idColor,
            string? tipoPeriodo,
            int? mes,
            int? anio,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            return new KardexFilterDto
            {
                // =============================
                // PRODUCTO
                // =============================
                IdProducto = idProducto,


                // =============================
                // CATÁLOGOS POR ID
                // =============================
                IdGenero = idGenero,

                IdReferencia = idReferencia,

                IdTalla = idTalla,

                IdTela = idTela,

                IdColor = idColor,


                // =============================
                // PERÍODO
                // =============================
                TipoPeriodo = tipoPeriodo,

                Mes = mes,

                Anio = anio,


                // =============================
                // FECHAS
                // =============================
                Desde = fechaInicio,

                Hasta = fechaFin
            };
        }

        // =========================================================
        // 📊 KARDEX (HISTORIAL CENTRAL)
        // =========================================================
        public async Task<IActionResult> Kardex(
            int? idProducto,
            int? idGenero,
            int? idReferencia,
            int? idTalla,
            int? idTela,
            int? idColor,
            string? tipoPeriodo,
            int? mes,
            int? anio,
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


            // =========================================================
            // 🔥 DTO FILTRO (CENTRALIZADO - SIN DUPLICACIÓN)
            // =========================================================
            var filter = BuildFilter(
                idProducto,
                idGenero,
                idReferencia,
                idTalla,
                idTela,
                idColor,
                tipoPeriodo,
                mes,
                anio,
                fechaInicio,
                fechaFin
            );


            // =========================================================
            // 🔥 SERVICIO (FUENTE ÚNICA DE VERDAD)
            // =========================================================
            var resultado = await _historialService
                .ObtenerKardexCompletoAsync(filter);


            // =========================================================
            // 🔥 DATOS PARA UI (FILTROS)
            // =========================================================

            ViewBag.Generos = await _context.Generos
                .AsNoTracking()
                .OrderBy(x => x.DescripGenero)
                .ToListAsync();


            
            ViewBag.Telas = await _context.Telas
                .AsNoTracking()
                .Where(x => x.Activo)
                .OrderBy(x => x.DescripTela)
                .ToListAsync();

            ViewBag.Colores = await _context.Colores
                 .AsNoTracking()
                 .Where(x => x.Activo)
                 .OrderBy(x => x.Nombre)
                 .ToListAsync();


            // =========================================================
            // 🔥 ESTADO DE FILTROS PARA LA VISTA KARDEX
            // Mantiene exactamente los mismos nombres usados en Razor
            // Kardex.cshtml
            // =========================================================

            ViewBag.Filtros = new
            {
                // =====================================================
                // PRODUCTO
                // =====================================================

                idProducto = idProducto,


                // =====================================================
                // CATÁLOGOS
                // =====================================================

                idGenero = idGenero,

                idReferencia = idReferencia,

                idTalla = idTalla,

                idTela = idTela,

                idColor = idColor,


                // =====================================================
                // PERÍODO
                // =====================================================

                tipoPeriodo = tipoPeriodo,

                mes = mes,

                anio = anio,


                // =====================================================
                // RANGO PERSONALIZADO
                // =====================================================

                fechaInicio = fechaInicio,

                fechaFin = fechaFin
            };

            // =========================================================
            // 📊 DATOS PARA LA VISTA
            // =========================================================

            // Gráfica
            ViewBag.Grafica = resultado.Grafica;


            // Indicadores superiores
            ViewBag.TotalEntradas = resultado.TotalEntradas;

            ViewBag.TotalSalidas = resultado.TotalSalidas;

            ViewBag.StockFinal = resultado.StockFinal;

            ViewBag.TotalMovimientos = resultado.TotalMovimientos;


            // Resumen mensual (tarjetas inferiores)
            ViewBag.ResumenMensual = resultado.ResumenMensual;


            // =========================================================
            // 📄 RETORNAR VISTA
            // =========================================================
            return View(resultado.Movimientos);
        }

                
        // =========================================================
        // 📊 GRÁFICA DINÁMICA KARDEX (DRILL-DOWN)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GraficaKardex(
            int? idProducto,
            int? idGenero,
            int? idReferencia,
            int? idTalla,
            int? idTela,
            int? idColor,
            string? tipoPeriodo,
            int? mes,
            int? anio,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            // =========================================================
            // 🔥 BUILDER CENTRALIZADO
            // =========================================================
            var filter = BuildFilter(
                idProducto,
                idGenero,
                idReferencia,
                idTalla,
                idTela,
                idColor,
                tipoPeriodo,
                mes,
                anio,
                fechaInicio,
                fechaFin
            );


            // =========================================================
            // 🔥 SERVICIO (FUENTE ÚNICA DE VERDAD)
            // =========================================================
            var resultado = await _historialService
                .ObtenerKardexCompletoAsync(filter);


            return Json(new
            {
                success = true,
                data = resultado.Grafica
            });
        }

        // =========================================================
        // AJAX: REFERENCIAS POR GÉNERO
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ReferenciasPorGenero(int id)
        {
            var referencias = await _context.Referencias
                .AsNoTracking()
                .Where(r => r.ID_Genero == id && r.Activo)
                .OrderBy(r => r.DescripReferencia)

                /*
                .Select(r => new
                {
                    Value = r.ID_Referencias,
                    Text = r.DescripReferencia
                })
                             */

                .Select(r => new
                {
                    id = r.ID_Referencias,
                    nombre = r.DescripReferencia
                })
                .ToListAsync();

            return Json(referencias);
        }

        // =========================================================
        // AJAX: TALLAS POR GÉNERO
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> TallasPorGenero(int id)
        {
            var tallas = await _context.Tallas
                .AsNoTracking()
                .Where(t => t.ID_Genero == id && t.Activo)
                .OrderBy(t => t.DescripTalla)

                /*
                .Select(t => new
                {
                    Value = t.ID_Tallas,
                    Text = t.DescripTalla
                })
                */
                              
                .Select(t => new
                {
                    id = t.ID_Tallas,
                    nombre = t.DescripTalla
                })
                .ToListAsync();

            return Json(tallas);
        }

        // =========================================================
        // 📦 EXPORTAR A EXCEL EL HISTORIAL
        // =========================================================
        public async Task<IActionResult> ExportarExcel(
            int? idProducto,
            int? idGenero,
            int? idReferencia,
            int? idTalla,
            int? idTela,
            int? idColor,
            string? tipoPeriodo,
            int? mes,
            int? anio,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            // =========================================================
            // 🔥 BUILDER CENTRALIZADO
            // =========================================================
            var filter = BuildFilter(
                idProducto,
                idGenero,
                idReferencia,
                idTalla,
                idTela,
                idColor,
                tipoPeriodo,
                mes,
                anio,
                fechaInicio,
                fechaFin
            );

            var resultado = await _historialService.ObtenerKardexCompletoAsync(filter);

            var datos = resultado.Movimientos;

            // =========================================================
            // 📘 CREAR LIBRO DE EXCEL
            // =========================================================
            using var workbook = new XLWorkbook();

            // =========================================================
            // 📄 PROPIEDADES DEL DOCUMENTO
            // =========================================================
            workbook.Properties.Author = ReportesConstantes.Sistema;
            workbook.Properties.Company = ReportesConstantes.Empresa;
            workbook.Properties.Title = ReportesConstantes.TituloKardex;
            workbook.Properties.Subject = "Reporte Oficial de Kardex de Inventario";
            workbook.Properties.Keywords = "Inventario, Kardex, Auditoría, Producción";
            workbook.Properties.Comments = ReportesConstantes.LeyendaAuditoria;

            // =========================================================
            // 📑 CREAR HOJA
            // =========================================================
            var hoja = workbook.Worksheets.Add("Kardex");

            // =========================================================
            // 🖼 LOGO INSTITUCIONAL
            // =========================================================

            var logoPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "img",
                "Logo.jpg");

            if (System.IO.File.Exists(logoPath))
            {
                hoja.AddPicture(logoPath)
                    .MoveTo(hoja.Cell("A1"), 15, 10)
                    .WithSize(140, 140);
            }


            // =========================================================
            // 🎨 CONFIGURACIÓN GENERAL DE LA HOJA
            // =========================================================
            hoja.Style.Font.FontName = "Calibri";
            hoja.Style.Font.FontSize = 12;

            // =========================================================
            // 📄 CONFIGURACIÓN DE IMPRESIÓN
            // =========================================================
            hoja.PageSetup.PageOrientation = XLPageOrientation.Landscape;

            hoja.PageSetup.PaperSize = XLPaperSize.LetterPaper;

            hoja.PageSetup.CenterHorizontally = true;

            hoja.PageSetup.Margins.Top = 0.50;
            hoja.PageSetup.Margins.Bottom = 0.50;
            hoja.PageSetup.Margins.Left = 0.35;
            hoja.PageSetup.Margins.Right = 0.35;


            // =========================================================
            // 📏 ANCHO INICIAL DE COLUMNAS
            // (Posteriormente se ajustarán automáticamente)
            // =========================================================

            for (int i = 1; i <= 16; i++)
            {
                hoja.Column(i).Width = 18;
            }
                        
            // =========================================================
            // 🏢 ENCABEZADO CORPORATIVO
            // =========================================================

            // Configuración de alturas del encabezado
            hoja.Row(1).Height = 30; // Empresa
            hoja.Row(2).Height = 22; // NIT
            hoja.Row(3).Height = 22; // Ciudad
            hoja.Row(4).Height = 22; // Actividad económica
            hoja.Row(5).Height = 22; // Sistema ERP
            hoja.Row(6).Height = 10; // Separación
            hoja.Row(7).Height = 26; // Título del reporte
            hoja.Row(8).Height = 22; // Subtítulo
            hoja.Row(9).Height = 8;  // Línea divisoria

            // ---------------------------------------------------------
            // Empresa
            // ---------------------------------------------------------

            hoja.Range("D1:O1").Merge();

            hoja.Cell("D1").Value = ReportesConstantes.Empresa;

            hoja.Cell("D1").Style.Font.Bold = true;
            hoja.Cell("D1").Style.Font.FontSize = 18;
            hoja.Cell("D1").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            hoja.Cell("D1").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            // ---------------------------------------------------------
            // NIT
            // ---------------------------------------------------------

            hoja.Range("D2:O2").Merge();

            hoja.Cell("D2").Value =
                $"NIT {ReportesConstantes.Nit}";

            hoja.Cell("D2").Style.Font.FontSize = 12;
            hoja.Cell("D2").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // ---------------------------------------------------------
            // Ciudad
            // ---------------------------------------------------------

            hoja.Range("D3:O3").Merge();

            hoja.Cell("D3").Value =
                ReportesConstantes.Ciudad;

            hoja.Cell("D3").Style.Font.FontSize = 12;
            hoja.Cell("D3").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // ---------------------------------------------------------
            // Actividad económica
            // ---------------------------------------------------------

            hoja.Range("D4:O4").Merge();

            hoja.Cell("D4").Value =
                ReportesConstantes.Actividad;

            hoja.Cell("D4").Style.Font.FontSize = 12;
            hoja.Cell("D4").Style.Font.Italic = true;
            hoja.Cell("D4").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // ---------------------------------------------------------
            // Sistema
            // ---------------------------------------------------------

            hoja.Range("D5:O5").Merge();

            hoja.Cell("D5").Value =
                ReportesConstantes.Sistema;

            hoja.Cell("D5").Style.Font.Bold = true;
            hoja.Cell("D5").Style.Font.FontSize = 12;
            hoja.Cell("D5").Style.Font.FontColor =
                XLColor.FromHtml("#1F4E79");

            hoja.Cell("D5").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // ---------------------------------------------------------
            // Título del reporte
            // ---------------------------------------------------------

            hoja.Range("A7:P7").Merge();

            hoja.Cell("A7").Value =
                ReportesConstantes.TituloKardex;

            hoja.Cell("A7").Style.Font.Bold = true;
            hoja.Cell("A7").Style.Font.FontSize = 16;
            hoja.Cell("A7").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            hoja.Cell("A7").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            // ---------------------------------------------------------
            // Subtítulo
            // ---------------------------------------------------------

            hoja.Range("A8:P8").Merge();

            hoja.Cell("A8").Value =
                ReportesConstantes.SubtituloKardex;

            hoja.Cell("A8").Style.Font.Italic = true;
            hoja.Cell("A8").Style.Font.FontSize = 12;
            hoja.Cell("A8").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // ---------------------------------------------------------
            // Línea divisoria
            // ---------------------------------------------------------

            hoja.Range("A9:P9")
                .Style.Border.BottomBorder =
                XLBorderStyleValues.Thick;

            // ---------------------------------------------------------
            // Marco del encabezado
            // ---------------------------------------------------------

            hoja.Range("A1:P9")
                .Style.Border.OutsideBorder =
                XLBorderStyleValues.Thick;
            // =========================================================
            // 📋 INFORMACIÓN DEL REPORTE
            // =========================================================

            hoja.Row(11).Height = 22;
            hoja.Row(12).Height = 22;
            hoja.Row(13).Height = 22;

            // Encabezado del bloque

            hoja.Range("A11:P11").Merge();

            hoja.Cell("A11").Value =
                "INFORMACIÓN DEL REPORTE";

            hoja.Cell("A11").Style.Font.Bold = true;
            hoja.Cell("A11").Style.Font.FontSize = 13;
            hoja.Cell("A11").Style.Font.FontColor = XLColor.White;
            hoja.Cell("A11").Style.Fill.BackgroundColor =
                XLColor.FromHtml("#1F4E79");

            hoja.Cell("A11").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Cell("A11").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;



            // =========================================================
            // INFORMACIÓN GENERAL Etiquetas
            // =========================================================

            hoja.Cell("A12").Value = "Código:";
            hoja.Cell("D12").Value = "Versión:";
            hoja.Cell("G12").Value = "Fecha de emisión:";
            hoja.Cell("L12").Value = "Sistema:";

            hoja.Cell("A13").Value = ReportesConstantes.CodigoKardex;
            hoja.Cell("D13").Value = ReportesConstantes.VersionKardex;
            hoja.Cell("G13").Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            hoja.Cell("L13").Value = ReportesConstantes.Sistema;

            // =========================================================
            // PERÍODO CONSULTADO
            // =========================================================

            hoja.Cell("A14").Value = "Período consultado:";
            hoja.Cell("E14").Value = "Desde:";
            hoja.Cell("I14").Value = "Hasta:";

            hoja.Cell("A15").Value = tipoPeriodo ?? "Personalizado";

            hoja.Cell("E15").Value =
                fechaInicio?.ToString("dd/MM/yyyy") ?? "-";

            hoja.Cell("I15").Value =
                fechaFin?.ToString("dd/MM/yyyy") ?? "-";

            // Negrilla en etiquetas
            hoja.Range("A14:I14").Style.Font.Bold = true;

            // Bordes del período
            hoja.Range("A14:P15").Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            hoja.Range("A14:P15").Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            // **********Formato***************

            hoja.Range("A12:P12")
                .Style.Font.Bold = true;

            hoja.Range("A12:P13")
                .Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            hoja.Range("A12:P13")
                .Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;


            // =========================================================
            // 📊 RESUMEN EJECUTIVO
            // =========================================================

            hoja.Range("A14:O14").Merge();

            hoja.Cell("A14").Value = "RESUMEN EJECUTIVO";

            hoja.Cell("A14").Style.Font.Bold = true;
            hoja.Cell("A14").Style.Font.FontSize = 13;
            hoja.Cell("A14").Style.Font.FontColor = XLColor.White;
            hoja.Cell("A14").Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
            hoja.Cell("A14").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Cell("A14").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Row(14).Height = 24;


            // =========================================================
            // TARJETA 1 - MOVIMIENTOS
            // =========================================================

            hoja.Range("A16:C16").Merge();
            hoja.Range("A17:C18").Merge();

            hoja.Cell("A16").Value = "MOVIMIENTOS";
            hoja.Cell("A17").Value = datos.Count;

            hoja.Range("A16:C16").Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
            hoja.Range("A16:C16").Style.Font.FontColor = XLColor.White;
            hoja.Range("A16:C16").Style.Font.Bold = true;
            hoja.Range("A16:C16").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("A16:C16").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("A17:C18").Style.Fill.BackgroundColor = XLColor.White;
            hoja.Range("A17:C18").Style.Font.Bold = true;
            hoja.Range("A17:C18").Style.Font.FontSize = 20;
            hoja.Range("A17:C18").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("A17:C18").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("A16:C18").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            hoja.Range("A16:C18").Style.Border.OutsideBorderColor = XLColor.FromHtml("#1F4E79");


            // =========================================================
            // TARJETA 2 - ENTRADAS
            // =========================================================

            hoja.Range("E16:G16").Merge();
            hoja.Range("E17:G18").Merge();

            hoja.Cell("E16").Value = "ENTRADAS";
            hoja.Cell("E17").Value = resultado.TotalEntradas;

            hoja.Range("E16:G16").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E7D32");
            hoja.Range("E16:G16").Style.Font.FontColor = XLColor.White;
            hoja.Range("E16:G16").Style.Font.Bold = true;
            hoja.Range("E16:G16").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("E16:G16").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("E17:G18").Style.Fill.BackgroundColor = XLColor.White;
            hoja.Range("E17:G18").Style.Font.Bold = true;
            hoja.Range("E17:G18").Style.Font.FontSize = 20;
            hoja.Range("E17:G18").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("E17:G18").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("E16:G18").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            hoja.Range("E16:G18").Style.Border.OutsideBorderColor = XLColor.FromHtml("#2E7D32");


            // =========================================================
            // TARJETA 3 - SALIDAS
            // =========================================================

            hoja.Range("I16:K16").Merge();
            hoja.Range("I17:K18").Merge();

            hoja.Cell("I16").Value = "SALIDAS";
            hoja.Cell("I17").Value = resultado.TotalSalidas;

            hoja.Range("I16:K16").Style.Fill.BackgroundColor = XLColor.FromHtml("#E67E22");
            hoja.Range("I16:K16").Style.Font.FontColor = XLColor.White;
            hoja.Range("I16:K16").Style.Font.Bold = true;
            hoja.Range("I16:K16").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("I16:K16").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("I17:K18").Style.Fill.BackgroundColor = XLColor.White;
            hoja.Range("I17:K18").Style.Font.Bold = true;
            hoja.Range("I17:K18").Style.Font.FontSize = 20;
            hoja.Range("I17:K18").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("I17:K18").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("I16:K18").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            hoja.Range("I16:K18").Style.Border.OutsideBorderColor = XLColor.FromHtml("#E67E22");


            // =========================================================
            // TARJETA 4 - STOCK FINAL
            // =========================================================

            hoja.Range("M16:O16").Merge();
            hoja.Range("M17:O18").Merge();

            hoja.Cell("M16").Value = "STOCK FINAL";
            hoja.Cell("M17").Value = resultado.StockFinal;

            hoja.Range("M16:O16").Style.Fill.BackgroundColor = XLColor.FromHtml("#7B1FA2");
            hoja.Range("M16:O16").Style.Font.FontColor = XLColor.White;
            hoja.Range("M16:O16").Style.Font.Bold = true;
            hoja.Range("M16:O16").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("M16:O16").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("M17:O18").Style.Fill.BackgroundColor = XLColor.White;
            hoja.Range("M17:O18").Style.Font.Bold = true;
            hoja.Range("M17:O18").Style.Font.FontSize = 20;
            hoja.Range("M17:O18").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Range("M17:O18").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            hoja.Range("M16:O18").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            hoja.Range("M16:O18").Style.Border.OutsideBorderColor = XLColor.FromHtml("#7B1FA2");

            // =========================================================
            // 📋 DETALLE DEL KARDEX
            // =========================================================

            const int filaCabecera = 21;
            

            // Encabezado
            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Fill.BackgroundColor =
                XLColor.FromHtml("#1F4E79");

            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Font.FontColor =
                XLColor.White;

            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Font.Bold = true;

            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Alignment.WrapText = true;

            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Border.OutsideBorder =
                XLBorderStyleValues.Medium;

            hoja.Range($"A{filaCabecera}:O{filaCabecera}")
                .Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            hoja.Row(filaCabecera).Height = 32;

            hoja.Cell(filaCabecera, 1).Value = "#";
            hoja.Cell(filaCabecera, 2).Value = "Fecha y Hora";
            hoja.Cell(filaCabecera, 3).Value = "Documento Origen";
            hoja.Cell(filaCabecera, 4).Value = "Tipo Movimiento";
            hoja.Cell(filaCabecera, 5).Value = "Descripción del Producto";
            hoja.Cell(filaCabecera, 6).Value = "Referencia Comercial";
            hoja.Cell(filaCabecera, 7).Value = "Talla";
            hoja.Cell(filaCabecera, 8).Value = "Tela";
            hoja.Cell(filaCabecera, 9).Value = "Color";
            hoja.Cell(filaCabecera,10).Value = "Stock Inicial";
            hoja.Cell(filaCabecera, 11).Value = "Entradas";
            hoja.Cell(filaCabecera, 12).Value = "Salidas";
            hoja.Cell(filaCabecera, 13).Value = "Stock Final";
            hoja.Cell(filaCabecera, 14).Value = "Usuario Responsable";
            hoja.Cell(filaCabecera, 15).Value = "Cliente";
            hoja.Cell(filaCabecera, 16).Value = "Observaciones";

            // =========================================================
            // 📦 DETALLE DE MOVIMIENTOS
            // =========================================================

            int fila = filaCabecera + 1;
            int consecutivo = 1;

            foreach (var item in datos)
            {
                // =====================================================
                // NÚMERO CONSECUTIVO
                // =====================================================

                hoja.Cell(fila, 1).Value = consecutivo;

                // =====================================================
                // DATOS DEL MOVIMIENTO
                // =====================================================

                hoja.Cell(fila, 2).Value = item.Fecha;
                hoja.Cell(fila, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                hoja.Cell(fila, 3).Value = item.DocumentoReferencia;
                hoja.Cell(fila, 4).Value = item.TipoMovimiento;
                hoja.Cell(fila, 5).Value = item.NombreProducto;
                hoja.Cell(fila, 6).Value = item.Referencia;
                hoja.Cell(fila, 7).Value = item.Talla;
                hoja.Cell(fila, 8).Value = item.Tela;
                hoja.Cell(fila, 9).Value = item.Color;

                hoja.Cell(fila, 10).Value = item.StockAnterior;
                hoja.Cell(fila, 11).Value = item.EntradaStock;
                hoja.Cell(fila, 12).Value = item.SalidaStock;
                hoja.Cell(fila, 13).Value = item.StockActual;

                hoja.Cell(fila, 14).Value = item.UsuarioNombre;
                hoja.Cell(fila, 15).Value = item.Cliente;
                hoja.Cell(fila, 16).Value = item.Observaciones;

                // =====================================================
                // FORMATO DE LA FILA
                // =====================================================

                hoja.Row(fila).Height = 22;

                hoja.Range(fila, 1, fila, 16)
                    .Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                consecutivo++;
                fila++;
            }

            // =========================================================
            // BORDES DEL DETALLE
            // =========================================================

            if (datos.Any())
            {
                hoja.Range(filaCabecera + 1, 1, fila - 1, 15)
                    .Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                hoja.Range(filaCabecera + 1, 1, fila - 1, 15)
                    .Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;
            }


            // =========================================================
            // 🎨 FORMATO DE FILAS (EFECTO CEBRA)
            // =========================================================

            for (int f = filaCabecera + 1; f < fila; f++)
            {
                if ((f - filaCabecera) % 2 == 0)
                {
                    hoja.Range(f, 1, f, 15)
                        .Style.Fill.BackgroundColor =
                        XLColor.FromHtml("#F8F9FA");
                }
            }

            // =========================================================
            // 🔢 FORMATO NUMÉRICO
            // =========================================================

            hoja.Column(9).Style.NumberFormat.Format = "#,##0";
            hoja.Column(10).Style.NumberFormat.Format = "#,##0";
            hoja.Column(11).Style.NumberFormat.Format = "#,##0";
            hoja.Column(12).Style.NumberFormat.Format = "#,##0";

            // =========================================================
            // 📐 ALINEACIÓN DE CANTIDADES
            // =========================================================

            for (int columna = 9; columna <= 12; columna++)
            {
                hoja.Column(columna).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Right;

                hoja.Column(columna).Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;
            }

            // =========================================================
            // 📊 TOTALES DEL REPORTE
            // =========================================================

            hoja.Cell(fila + 1, 8).Value = "TOTALES";

            hoja.Cell(fila + 1, 8).Style.Font.Bold = true;

            hoja.Cell(fila + 1, 10).Value = resultado.TotalEntradas;

            hoja.Cell(fila + 1, 11).Value = resultado.TotalSalidas;

            hoja.Cell(fila + 1, 12).Value = resultado.StockFinal;

            hoja.Range(fila + 1, 8, fila + 1, 12)
                .Style.Fill.BackgroundColor =
                XLColor.FromHtml("#D9EAD3");

            hoja.Range(fila + 1, 8, fila + 1, 12)
                .Style.Font.Bold = true;

            hoja.Range(fila + 1, 8, fila + 1, 12)
                .Style.Border.OutsideBorder =
                XLBorderStyleValues.Thick;

            // =========================================================
            // 📝 PIE DEL REPORTE
            // =========================================================

            int filaPie = fila + 4;

            hoja.Range(filaPie, 1, filaPie, 15).Merge();

            hoja.Cell(filaPie, 1).Value =
                ReportesConstantes.LeyendaAuditoria;

            hoja.Cell(filaPie, 1)
                .Style.Alignment.WrapText = true;

            hoja.Cell(filaPie, 1)
                .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Left;

            hoja.Cell(filaPie, 1)
                .Style.Font.Italic = true;

            hoja.Cell(filaPie, 1)
                .Style.Font.FontColor =
                XLColor.DarkGray;

            hoja.Row(filaPie).Height = 45;

            // =========================================================
            // 🖨 CONFIGURACIÓN FINAL DE IMPRESIÓN
            // =========================================================

            hoja.PageSetup.PagesWide = 1;

            hoja.PageSetup.PagesTall = 0;

            hoja.PageSetup.CenterHorizontally = true;
            hoja.PageSetup.SetRowsToRepeatAtTop($"{filaCabecera}:{filaCabecera}");


            // =========================================================
            // 📏 AJUSTAR ANCHO DE COLUMNAS
            // =========================================================

            hoja.Column(1).Width = 5;      // #
            hoja.Column(2).Width = 20;     // Fecha y Hora
            hoja.Column(3).Width = 15;     // Documento Origen
            hoja.Column(4).Width = 18;     // Tipo Movimiento
            hoja.Column(5).Width = 42;     // Descripción del Producto
            hoja.Column(6).Width = 18;     // Referencia Comercial
            hoja.Column(7).Width = 8;      // Talla
            hoja.Column(8).Width = 18;     // Tela
            hoja.Column(9).Width = 12;     // Color
            hoja.Column(10).Width = 12;    // Stock Inicial
            hoja.Column(11).Width = 10;    // Entradas
            hoja.Column(12).Width = 10;    // Salidas
            hoja.Column(13).Width = 12;    // Stock Final
            hoja.Column(14).Width = 20;    // Usuario Responsable
            hoja.Column(15).Width = 24;    // Cliente
            hoja.Column(16).Width = 28;    // Observaciones

            // =========================================================
            // BORDES DE TODO EL DETALLE
            // =========================================================

            if (datos.Any())
            {
                var rangoDetalle = hoja.Range(
                    filaCabecera,
                    1,
                    fila - 1,
                    16);

                rangoDetalle.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rangoDetalle.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rangoDetalle.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rangoDetalle.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                rangoDetalle.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;
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