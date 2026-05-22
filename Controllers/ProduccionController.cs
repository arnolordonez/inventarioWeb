using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.Services;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace InventarioWEB.Controllers
{
     [AllowAnonymous]
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
        // GET CREAR PRODUCCION MANUAL
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

                    model.Estado = pedido.Estado;

                    model.EstadoPago = pedido.EstadoPago;

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
                        // =========================================
                        // IDENTIFICADORES
                        // =========================================

                        ID_Producto = x.ID_Producto,

                        ID_DetallePedido = x.ID_DetallePedido,

                        // =========================================
                        // INFORMACIÓN VISUAL ERP
                        // =========================================

                        NombreProducto = x.Producto,

                        Referencia = x.Referencia,

                        Talla = x.Talla,

                        Color = x.Color,

                        // =========================================
                        // PRODUCCIÓN
                        // =========================================

                        CantidadPedido = x.CantidadPedido,

                        CantidadProducidaActual = x.CantidadProducida,

                        CantidadPendiente = x.Pendiente,

                        StockActual = x.StockActual,

                        // =========================================
                        // INPUTS USUARIO
                        // =========================================

                        CantidadProducida = 0,

                        CostoUnitario = x.PrecioCosto,

                        // =========================================
                        // PRECIOS
                        // =========================================

                        PrecioVentaUnitario = x.PrecioVTA,

                        IVA = x.IVA_Porcentaje
                    })
                    .ToList();
            }

            await CargarFiltrosBaseAsync();

            return View(model);
        }

        // ==========================================================
        // GET CREAR PRODUCCIÓN ERP
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> CrearERP(int idPedido)
        {                   
            var model = new ProduccionCrearViewModel
            {
                FechaProduccion = DateTime.Today,
                Detalles = new List<DetalleProduccionVM>()
            };

            // =====================================================
            // PEDIDO
            // =====================================================

            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.ID_Pedido == idPedido);

            if (pedido == null)
            {
                return NotFound();
            }

            model.ID_Pedido = pedido.ID_Pedido;

            model.ID_Cliente = pedido.ID_Cliente;

            model.Cliente =
                pedido.Cliente != null
                    ? pedido.Cliente.Nombre + " " + pedido.Cliente.Apellido
                    : string.Empty;

            model.Estado = pedido.Estado;

            model.EstadoPago = pedido.EstadoPago;

            model.TipoVenta = pedido.TipoVenta;

            model.TotalPedido = pedido.Total;

            model.SaldoPendiente = pedido.Saldo;

            // =====================================================
            // DETALLE ERP
            // =====================================================

            var detallesPedido =
                await _produccionService
                    .ObtenerDetallePedidoParaProduccionAsync(idPedido);

            model.Detalles = detallesPedido
                .Select(x => new DetalleProduccionVM
                {
                    // =========================================
                    // IDENTIFICADORES
                    // =========================================

                    ID_Producto = x.ID_Producto,

                    ID_DetallePedido = x.ID_DetallePedido,

                    // =========================================
                    // INFORMACIÓN VISUAL ERP
                    // =========================================

                    NombreProducto = x.Producto,

                    Referencia = x.Referencia,

                    Talla = x.Talla,

                    Color = x.Color,

                    // =========================================
                    // PRODUCCIÓN
                    // =========================================

                    CantidadPedido = x.CantidadPedido,

                    CantidadProducidaActual = x.CantidadProducida,

                    CantidadPendiente = x.Pendiente,

                    StockActual = x.StockActual,

                    // =========================================
                    // INPUTS USUARIO
                    // =========================================

                    CantidadProducida = 0,

                    CostoUnitario = x.PrecioCosto > 0
                    ? x.PrecioCosto
                    : x.PrecioVTA,

                    // =========================================
                    // PRECIOS
                    // =========================================

                    PrecioVentaUnitario = x.PrecioVTA,

                    IVA = x.IVA_Porcentaje
                })
                .ToList();

            return View("CrearERP", model);
        }

        // ==========================================================
        // POST CREAR PRODUCCIÓN ERP
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearERP(ProduccionCrearViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CrearERP", model);
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =====================================================
                // FILTRAR SOLO ITEMS CON PRODUCCIÓN
                // =====================================================

                var itemsProduccion = model.Detalles
                    .Where(x => x.CantidadProducida > 0)
                    .ToList();

                if (!itemsProduccion.Any())
                {
                    throw new Exception(
                        "Debe ingresar al menos una cantidad a producir."
                    );
                }

                // =====================================================
                // CREAR UNA SOLA CABECERA PRODUCCIÓN ERP
                // =====================================================

                var produccion = new Produccion
                {
                    FechaProduccion = model.FechaProduccion,
                    Observacion = "Producción ERP",
                    Usuario = User?.Identity?.Name ?? "Sistema",
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                _context.Producciones.Add(produccion);

                await _context.SaveChangesAsync();

                // =====================================================
                // RECORRER DETALLES
                // =====================================================

                foreach (var item in itemsProduccion)
                {
                    // ================================================
                    // VALIDAR PRODUCTO
                    // ================================================

                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p =>
                            p.ID_Producto == item.ID_Producto);

                    if (producto == null)
                    {
                        throw new Exception(
                            $"Producto no encontrado ID {item.ID_Producto}"
                        );
                    }

                    // ================================================
                    // VALIDAR PENDIENTE REAL DESDE BD
                    // ================================================

                    var detallePedido = await _context.DetallePedidos
                        .FirstOrDefaultAsync(d =>
                            d.ID_Detalle == item.ID_DetallePedido);

                    if (detallePedido == null)
                    {
                        throw new Exception(
                            $"Detalle pedido no encontrado para el producto {item.NombreProducto}"
                        );
                    }

                    var totalProducido =
                        await _context.DetalleProducciones
                            .Where(x =>
                                x.ID_DetallePedido == item.ID_DetallePedido)
                            .SumAsync(x =>
                                (int?)x.CantidadProducida) ?? 0;

                    var pendienteReal =
                        detallePedido.Cantidad - totalProducido;

                    if (pendienteReal < 0)
                    {
                        pendienteReal = 0;
                    }

                    // ================================================
                    // VALIDAR PENDIENTE
                    // ================================================

                    if (item.CantidadProducida > pendienteReal)
                    {
                        throw new Exception(
                            $"La producción supera lo pendiente del producto {item.NombreProducto}. " +
                            $"Pendiente actual: {pendienteReal}"
                        );
                    }

                    // ================================================
                    // CREAR DETALLE PRODUCCIÓN
                    // ================================================

                    var detalleProduccion = new DetalleProduccion
                    {
                        ID_Produccion = produccion.ID_Produccion,

                        ID_Producto = item.ID_Producto,

                        ID_DetallePedido = item.ID_DetallePedido,

                        CantidadProducida = item.CantidadProducida,

                        CostoUnitario = item.CostoUnitario,

                        PrecioVentaUnitario = item.PrecioVentaUnitario,

                        IVA = item.IVA,

                        SubtotalCosto =
                            item.CantidadProducida *
                            item.CostoUnitario,

                        SubtotalVenta =
                            item.CantidadProducida *
                            item.PrecioVentaUnitario,

                        EstadoProduccion = "TERMINADO",

                        FechaInicioProduccion = DateTime.Now,

                        FechaFinProduccion = DateTime.Now,

                        ObservacionProduccion = "Producción ERP"
                    };

                    _context.DetalleProducciones.Add(detalleProduccion);

                    // ================================================
                    // ACTUALIZAR INVENTARIO
                    // ================================================

                    producto.Stock += item.CantidadProducida;

                    _context.Productos.Update(producto);
                }
                // =====================================================
                // GUARDAR TODO
                // =====================================================

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Producción registrada correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError("", ex.Message);

                return View("CrearERP", model);
            }
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
