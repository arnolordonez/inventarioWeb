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
        // LISTADO DE PRODUCCIONES SEGUN PEDIDO
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            var pedidos =
                await _produccionService
                    .ObtenerDashboardPedidosProduccionAsync();

            var model = new ProduccionPedidoDashboardVM
            {
                Pedidos = pedidos,

                TotalPedidosPendientes =
                 pedidos.Count(x => x.EstadoProduccion == "PENDIENTE"),

                        TotalPedidosEnProduccion =
                 pedidos.Count(x => x.EstadoProduccion == "EN PRODUCCIÓN"),

                        TotalPedidosCompletados =
                 pedidos.Count(x => x.EstadoProduccion == "COMPLETADO"),

                        TotalUnidadesPendientes =
                 pedidos.Sum(x => x.Pendiente),

                        TotalUnidadesProducidas =
                 pedidos.Sum(x => x.TotalProducido)
            };

            return View(model);
        }
        // ==========================================================
        // GET CREAR PRODUCCION
        // ==========================================================
        public async Task<IActionResult> Crear(int? idPedido)
        {
            var model = new ProduccionCrearViewModel
            {
                FechaProduccion = DateTime.Today,
                Detalles = new List<DetalleProduccionVM>()
            };

            if (idPedido.HasValue)
            {
                var pedido = await _context.Pedidos
                    .AsNoTracking()
                    .Include(p => p.Cliente)
                    .FirstOrDefaultAsync(p => p.ID_Pedido == idPedido.Value);

                if (pedido != null)
                {
                    model.ID_Pedido = pedido.ID_Pedido;

                    model.ID_Cliente = pedido.ID_Cliente;

                    model.Cliente =
                        pedido.Cliente != null
                            ? pedido.Cliente.Nombre + " " + pedido.Cliente.Apellido
                            : string.Empty;

                    // =====================================================
                    // ESTADOS
                    // =====================================================

                    model.Estado = pedido.Estado;

                    model.EstadoPago = pedido.EstadoPago;

                    // =====================================================
                    // VENTA
                    // =====================================================

                    model.TipoVenta = pedido.TipoVenta;

                    model.TotalPedido = pedido.Total;

                    model.SaldoPendiente = pedido.Saldo;
                }

                  var detallesPedido =
                      await _produccionService
                      .ObtenerDetallePedidoParaProduccionAsync(idPedido.Value);

                   model.Detalles = detallesPedido
                    .Select(x => new DetalleProduccionVM
                    {
                        ID_Producto = x.ID_Producto,

                        // 👇 IMPORTANTE: empieza en 0 (input del usuario)
                        CantidadProducida = 0,

                        // 👇 lo necesitas para validar luego
                        CantidadPendiente = x.Pendiente,

                        CostoUnitario = 0,

                        ID_DetallePedido = x.ID_DetallePedido
                    })
                    .ToList();
            }

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
                .Where(d => d.ID_Producto > 0 && d.CantidadProducida > 0 && d.CostoUnitario > 0)
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

                // =====================================================
                // NORMALIZAR CANTIDAD
                // =====================================================

                var cantidadProducida = d.CantidadProducida;

                if (cantidadProducida <= 0)
                    continue;

                // =====================================================
                // NORMALIZAR COSTO
                // =====================================================

                decimal costoUnitario = d.CostoUnitario;

                // Evita guardar 240000 en lugar de 2400
                if (costoUnitario > 10000)
                    costoUnitario = costoUnitario / 100;

                costoUnitario =
                    Math.Round(costoUnitario, 2, MidpointRounding.AwayFromZero);

                // =====================================================
                // CÁLCULOS
                // =====================================================

                var subtotalCosto =
                    Math.Round(
                        cantidadProducida * costoUnitario,
                        2,
                        MidpointRounding.AwayFromZero);

                var subtotalVenta =
                    Math.Round(
                        cantidadProducida * producto.PrecioVTA,
                        2,
                        MidpointRounding.AwayFromZero);

                // =====================================================
                // DETALLE PRODUCCIÓN
                // =====================================================

                detalles.Add(new DetalleProduccion
                {
                    ID_Producto = d.ID_Producto,

                    ID_DetallePedido = d.ID_DetallePedido,

                    CantidadProducida = cantidadProducida,

                    CostoUnitario = costoUnitario,

                    PrecioVentaUnitario = producto.PrecioVTA,

                    IVA = producto.IVA_Porcentaje,

                    SubtotalCosto = subtotalCosto,

                    SubtotalVenta = subtotalVenta,

                    EstadoProduccion = "PENDIENTE",

                    FechaInicioProduccion = null,

                    FechaFinProduccion = null,

                    ObservacionProduccion = null
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

                stock = p.Stock,

                // =========================================
                // PRODUCCIÓN
                // =========================================

                cantidadPedido = p.CantidadPedido,

                cantidadProducida = p.CantidadProducida,

                cantidadPendiente = p.CantidadPendiente,

                idDetallePedido = p.ID_DetallePedido,

                iva = p.IVA
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
                    CantidadProducida = d.CantidadProducida,
                    CostoUnitario = d.CostoUnitario,
                    PrecioVentaUnitario = d.PrecioVentaUnitario,
                    IVA = d.IVA,
                    SubtotalCosto = d.SubtotalCosto,
                    SubtotalVenta = d.SubtotalVenta
                })
                .AsNoTracking()
                .ToListAsync();

            var totalCantidad = detalles.Sum(x => x.CantidadProducida);
            var totalCosto = detalles.Sum(x => x.SubtotalCosto);
            var totalVenta = detalles.Sum(x => x.SubtotalVenta);

            var vm = new ReporteProduccionViewModel
            {
                Produccion = produccion ?? new Produccion(),
                Detalles = detalles,

                TotalCantidadProducida = totalCantidad,
                TotalCosto = totalCosto,
                TotalVenta = totalVenta,
                MargenBrutoEstimado = totalVenta - totalCosto
            };

            return View("ReporteProduccionPdf", vm);
        }
    }
}
