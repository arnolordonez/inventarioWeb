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
        // 📄 INDEX - LISTADO DE PRODUCCIONES 
        // ==========================================================
        public async Task<IActionResult> Index(int pagina = 1, int registrosPorPagina = 20)
        {
            // 🔹 CARGAR COMBOS PARA FILTROS
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
        // ➕ GET: CREAR PRODUCCIÓN
        // ==========================================================
        public async Task<IActionResult> Crear()
        {
            var model = new ProduccionCrearViewModel
            {
                FechaProduccion = DateTime.Today
            };

            await CargarFiltrosBaseAsync();
            return View(model);
        }

        // ==========================================================
        // 💾 POST: CREAR PRODUCCIÓN
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProduccionCrearViewModel model)
        {
            model.Detalles = model.Detalles?
                .Where(d => d.ID_Producto > 0 && d.Cantidad > 0)
                .ToList() ?? new List<DetalleProduccionVM>();

            if (!model.Detalles.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto válido.");
            }

            var idsProductos = model.Detalles
                .Select(d => d.ID_Producto)
                .Distinct()
                .ToList();

            var productosBD = await _context.Productos
                .AsNoTracking()
                .Where(p => idsProductos.Contains(p.ID_Producto))
                .ToListAsync();

            foreach (var detalle in model.Detalles)
            {
                var producto = productosBD
                    .FirstOrDefault(p => p.ID_Producto == detalle.ID_Producto);

                if (producto == null)
                {
                    ModelState.AddModelError("", $"El producto con ID {detalle.ID_Producto} no existe.");
                    continue;
                }

                if (!producto.Activo)
                {
                    ModelState.AddModelError("", $"El producto {producto.Nombre} está inactivo.");
                }

                if (detalle.CostoUnitario <= 0)
                {
                    ModelState.AddModelError("", $"El costo del producto {producto.Nombre} debe ser mayor a cero.");
                }
            }

            if (!ModelState.IsValid)
            {
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

            var detalles = model.Detalles.Select(d => new DetalleProduccion
            {
                ID_Producto = d.ID_Producto,
                Cantidad = d.Cantidad,
                CostoUnitario = d.CostoUnitario
            }).ToList();

            try
            {
                await _produccionService.RegistrarProduccionAsync(produccion, detalles);
                TempData["Success"] = "Producción registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ocurrió un error al registrar la producción.");
                await CargarFiltrosBaseAsync();
                return View(model);
            }
        }

        // ==========================================================
        // 🔍 BÚSQUEDA AJAX DE PRODUCTOS (CONTROLADA)
        // ==========================================================
        // ==========================================================
        // 🔍 BÚSQUEDA AJAX DE PRODUCTOS (CONTROLADA)
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(
    int? idProducto,
    int? idGenero,
    int? idReferencia,
    int? idTalla,
    int? idTela,
    int? idColor)
        {
            Console.WriteLine("==== CHECK CONTROLLER ====");
            Console.WriteLine($"idGenero: {idGenero}");
            Console.WriteLine($"idReferencia: {idReferencia}");
            Console.WriteLine($"idTalla: {idTalla}");
            Console.WriteLine($"idTela: {idTela}");
            Console.WriteLine($"idColor: {idColor}");
            Console.WriteLine("==========================");
            var query = _context.Productos
    .Include(p => p.Referencia)
    .Include(p => p.Talla)
    .AsQueryable();

            if (idProducto.HasValue && idProducto.Value > 0)
            {
                query = query.Where(p => p.ID_Producto == idProducto.Value);
            }
            else
            {
                if (idGenero.HasValue && idGenero.Value > 0)
                    query = query.Where(p => p.Referencia.ID_Genero == idGenero.Value);

                if (idReferencia.HasValue && idReferencia.Value > 0)
                    query = query.Where(p => p.ID_Referencias == idReferencia.Value);

                if (idTalla.HasValue && idTalla.Value > 0)
                    query = query.Where(p => p.ID_Tallas == idTalla.Value);

                if (idTela.HasValue && idTela.Value > 0)
                    query = query.Where(p => p.ID_Telas == idTela.Value);

                if (idColor.HasValue && idColor.Value > 0)
                    query = query.Where(p => p.ID_Color == idColor.Value);
            }

            // ============================
            // PROYECCIÓN
            // ============================
            var sql = query.ToQueryString();
            Console.WriteLine("==== SQL GENERADO ====");
            Console.WriteLine(sql);
            Console.WriteLine("======================");

            var resultado = await query
                .Select(p => new
                {
                    idProducto = p.ID_Producto,
                    nombre = p.Nombre,
                    precioCosto = p.PrecioCosto,
                    precioVTA = p.PrecioVTA,
                    stock = p.Stock,
                    iva = p.IVA_Porcentaje,
                    activo = p.Activo,
                    genero = p.Genero.DescripGenero,
                    referencia = p.Referencia.DescripReferencia,
                    talla = p.Talla.DescripTalla
                })
                .ToListAsync();

            return Json(resultado);
        }
        // ==========================================================
        // 🔄 REFERENCIAS POR GÉNERO (AJAX)
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> ObtenerReferenciasPorGenero(int idGenero)
        {
            if (idGenero <= 0)
                return Json(new List<object>());

            var data = await _produccionService.ObtenerReferenciasPorGenero(idGenero);
            return Json(data);
        }
        // ==========================================================
        // 🔄 TALLAS POR GÉNERO (AJAX)
        // ==========================================================
        
        [HttpGet]
        public async Task<IActionResult> ObtenerTallasPorGenero(int idGenero)
        {
            if (idGenero <= 0)
                return Json(new List<object>());

            var data = await _produccionService.ObtenerTallasPorGenero(idGenero);
            return Json(data);
        }

        // ==========================================================
        // 📌 CARGA BASE DE CATÁLOGOS (SIN REFERENCIAS NI TALLAS)
        // ==========================================================
        // ==========================================================
        // 📌 CARGA BASE DE CATÁLOGOS (SIN REFERENCIAS NI TALLAS)
        // ==========================================================
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
    }
}