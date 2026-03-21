using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.Services;
using InventarioWEB.ViewModels;

namespace InventarioWEB.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly MovimientoVentasDbContext _context;
        private readonly ProduccionService _produccionService;

        public ProduccionController(
            MovimientoVentasDbContext context,
            ProduccionService produccionService)
        {
            _context = context;
            _produccionService = produccionService;
        }

        // ==========================================================
        // LISTADO DE PRODUCCIONES
        // ==========================================================
        public async Task<IActionResult> Index(int pagina = 1, int registrosPorPagina = 20)
        {
            await CargarFiltrosBaseAsync();

            var query = _context.Producciones
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaProduccion)
                .ThenByDescending(p => p.ID_Produccion);

            var totalRegistros = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0)
                pagina = totalPaginas;

            var producciones = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            var model = new ProduccionViewModel
            {
                Producciones = producciones,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }

        // ==========================================================
        // GET CREAR PRODUCCION
        // ==========================================================
        public async Task<IActionResult> Crear()
        {
            var model = new ProduccionCrearViewModel
            {
                FechaProduccion = DateTime.Today,
                Detalles = new List<DetalleProduccionVM>()
            };

            await CargarFiltrosBaseAsync();
            return View(model);
        }

        // ==========================================================
        // POST CREAR PRODUCCION
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProduccionCrearViewModel model)
        {
            model.Detalles = model.Detalles?
                .Where(d => d.ID_Producto > 0 && d.Cantidad > 0 && d.CostoUnitario > 0)
                .ToList() ?? new List<DetalleProduccionVM>();

            if (!model.Detalles.Any())
            {
                ModelState.AddModelError("", "Debe ingresar al menos un producto con cantidad mayor a cero.");
            }

            if (!ModelState.IsValid)
            {
                await CargarFiltrosBaseAsync();
                return View(model);
            }

            var idsProductos = model.Detalles
                .Select(d => d.ID_Producto)
                .Distinct()
                .ToList();

            var productosBD = await _context.Productos
                .AsNoTracking()
                .Where(p => idsProductos.Contains(p.ID_Producto))
                .ToListAsync();

            if (productosBD.Count != idsProductos.Count)
            {
                ModelState.AddModelError("", "Uno o más productos no existen.");
                await CargarFiltrosBaseAsync();
                return View(model);
            }

            var produccion = new Produccion
            {
                FechaProduccion = model.FechaProduccion,
                Observacion = model.Observaciones,
                Activo = true,
                FechaRegistro = DateTime.Now,
                Usuario = User?.Identity?.Name ?? "Sistema"
            };

            var productosDict = productosBD.ToDictionary(p => p.ID_Producto);

            var detalles = new List<DetalleProduccion>();

            foreach (var d in model.Detalles)
            {
                if (!productosDict.TryGetValue(d.ID_Producto, out var producto))
                    continue;

                // Corrección de costo si viene multiplicado por 100 desde el frontend
                decimal costoUnitario = d.CostoUnitario;

                if (costoUnitario > 10000) // evita guardar 240000 en lugar de 2400
                    costoUnitario = costoUnitario / 100;

                costoUnitario = Math.Round(costoUnitario, 2, MidpointRounding.AwayFromZero);

                var subtotalCosto = Math.Round(d.Cantidad * costoUnitario, 2, MidpointRounding.AwayFromZero);
                var subtotalVenta = Math.Round(d.Cantidad * producto.PrecioVTA, 2, MidpointRounding.AwayFromZero);

                detalles.Add(new DetalleProduccion
                {
                    ID_Producto = d.ID_Producto,
                    Cantidad = d.Cantidad,
                    CostoUnitario = costoUnitario,
                    PrecioVentaUnitario = producto.PrecioVTA,
                    IVA = producto.IVA_Porcentaje,
                    SubtotalCosto = subtotalCosto,
                    SubtotalVenta = subtotalVenta
                });
            }

            try
            {
                await _produccionService
                    .RegistrarProduccionAsync(produccion, detalles);

                TempData["Success"] = "Producción registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al registrar la producción.");
                await CargarFiltrosBaseAsync();
                return View(model);
            }
        }

        // ==========================================================
        // BUSCAR PRODUCTOS (AJAX)
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> BuscarProductos(
            int? idProducto,
            int? idGenero,
            int? idReferencia,
            int? idTalla,
            int? idTela,
            int? idColor,
            int pagina = 1,
            int registrosPorPagina = 50)
        {
            var (lista, total) = await _produccionService.BuscarProductosAsync(
                idProducto,
                idGenero,
                idReferencia,
                idTalla,
                idTela,
                idColor,
                pagina,
                registrosPorPagina);

            var resultado = lista.Select(p => new
            {
                idProducto = p.ID_Producto,
                nombre = p.Nombre,
                genero = p.Genero,
                referencia = p.Referencia,
                talla = p.Talla,
                tela = p.Tela,
                color = p.Color,
                precioCosto = p.PrecioCosto,
                precioVTA = p.PrecioVTA,
                stock = p.Stock
            });

            return Json(resultado);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerReferenciasPorGenero(int idGenero)
        {
            if (idGenero <= 0)
                return Json(new List<object>());

            var data = await _produccionService.ObtenerReferenciasPorGenero(idGenero);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTallasPorGenero(int idGenero)
        {
            if (idGenero <= 0)
                return Json(new List<object>());

            var data = await _produccionService.ObtenerTallasPorGenero(idGenero);
            return Json(data);
        }

        private async Task CargarFiltrosBaseAsync()
        {
            ViewBag.Generos = new SelectList(
                await _context.Generos
                    .AsNoTracking()
                    .OrderBy(g => g.DescripGenero)
                    .ToListAsync(),
                "ID_Genero",
                "DescripGenero");

            ViewBag.Telas = new SelectList(
                await _context.Telas
                    .AsNoTracking()
                    .Where(t => t.Activo)
                    .OrderBy(t => t.DescripTela)
                    .ToListAsync(),
                "ID_Telas",
                "DescripTela");

            ViewBag.Colores = new SelectList(
                await _context.Colores
                    .AsNoTracking()
                    .Where(c => c.Activo)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync(),
                "ID_Color",
                "Nombre");
        }

        public async Task<IActionResult> ReporteProduccionPdf(int id)
        {
            var produccion = await _context.Producciones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID_Produccion == id);

            if (produccion == null)
                return NotFound();

            var detalles = await (
                from d in _context.DetalleProducciones
                join p in _context.Productos
                    on d.ID_Producto equals p.ID_Producto
                where d.ID_Produccion == id
                select new DetalleProduccionReporteVM
                {
                    ID_Producto = d.ID_Producto,
                    NombreProducto = p.Nombre,
                    Cantidad = d.Cantidad,
                    CostoUnitario = d.CostoUnitario,
                    PrecioVentaUnitario = d.PrecioVentaUnitario,
                    IVA = d.IVA,
                    SubtotalCosto = d.SubtotalCosto,
                    SubtotalVenta = d.SubtotalVenta
                })
                .AsNoTracking()
                .ToListAsync();

            var vm = new ReporteProduccionViewModel
            {
                Produccion = produccion,
                Detalles = detalles
            };

            return View("ReporteProduccionPdf", vm);
        }
    }
}
