using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventarioWEB.Services
{
    public class ProduccionService
    {
        private readonly MovimientoVentasDbContext _context;


        private readonly decimal _margenAdvertencia;
        private readonly decimal _margenCritico;

        public ProduccionService(MovimientoVentasDbContext context, IConfiguration config)
        {
            _context = context;

            _margenAdvertencia = config.GetValue<decimal>("Produccion:MargenAdvertencia");
            _margenCritico = config.GetValue<decimal>("Produccion:MargenCritico");
        }
       

        // ============================================================
        // 🔍 BÚSQUEDA OPTIMIZADA DE PRODUCTOS PARA PRODUCCIÓN
        // ============================================================

        public async Task<(List<ProductoProduccionDTO> Lista, int Total)>
        BuscarProductosAsync(
            int? idProducto,
            int? idGenero,
            int? idReferencia,
            int? idTalla,
            int? idTela,
            int? idColor,
            int pagina = 1,
            int registrosPorPagina = 50)
        {
            // FAST PATH por PK
            if (idProducto.HasValue && idProducto.Value > 0)
            {
                var pkQuery =
                    from p in _context.Productos.AsNoTracking()
                    join r in _context.Referencias on p.ID_Referencias equals r.ID_Referencias
                    join g in _context.Generos on p.ID_Genero equals g.ID_Genero

                    join t in _context.Tallas on p.ID_Tallas equals t.ID_Tallas
                    join te in _context.Telas on p.ID_Telas equals te.ID_Telas
                    join c in _context.Colores on p.ID_Color equals c.ID_Color
                    where p.ID_Producto == idProducto.Value && p.Activo
                    select new ProductoProduccionDTO
                    {
                        ID_Producto = p.ID_Producto,
                        Nombre = p.Nombre,
                        Genero = g.DescripGenero,        // ✅ NUEVO CAMPO

                        Referencia = r.DescripReferencia,
                        Talla = t.DescripTalla,
                        Tela = te.DescripTela,
                        Color = c.Nombre,
                        Stock = p.Stock,
                        PrecioCosto = p.PrecioCosto,
                        PrecioVTA = p.PrecioVTA
                    };

                var pk = await pkQuery.ToListAsync();
                return (pk, pk.Count);
            }

            // Evita full scan
            if (!idGenero.HasValue &&
                !idReferencia.HasValue &&
                !idTalla.HasValue &&
                !idTela.HasValue &&
                !idColor.HasValue)
            {
                return (new List<ProductoProduccionDTO>(), 0);
            }

            var baseQuery = _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo);

            if (idGenero.HasValue)
                baseQuery = baseQuery.Where(p => p.ID_Genero == idGenero.Value);

            if (idReferencia.HasValue)
                baseQuery = baseQuery.Where(p => p.ID_Referencias == idReferencia.Value);

            if (idTalla.HasValue)
                baseQuery = baseQuery.Where(p => p.ID_Tallas == idTalla.Value);

            if (idTela.HasValue)
                baseQuery = baseQuery.Where(p => p.ID_Telas == idTela.Value);

            if (idColor.HasValue)
                baseQuery = baseQuery.Where(p => p.ID_Color == idColor.Value);

            var total = await baseQuery.CountAsync();

            if (total == 0)
                return (new List<ProductoProduccionDTO>(), 0);

            var lista = await (
                from p in baseQuery
                join r in _context.Referencias on p.ID_Referencias equals r.ID_Referencias
                join g in _context.Generos on p.ID_Genero equals g.ID_Genero   // ✅ AGREGADO
                join t in _context.Tallas on p.ID_Tallas equals t.ID_Tallas
                join te in _context.Telas on p.ID_Telas equals te.ID_Telas
                join c in _context.Colores on p.ID_Color equals c.ID_Color
                orderby p.ID_Referencias, p.ID_Tallas, p.ID_Telas, p.ID_Color
                select new ProductoProduccionDTO
                {
                    ID_Producto = p.ID_Producto,
                    Nombre = p.Nombre,
                    Genero = g.DescripGenero,        // ✅ AGREGADO
                    Referencia = r.DescripReferencia,
                    Talla = t.DescripTalla,
                    Tela = te.DescripTela,
                    Color = c.Nombre,
                    Stock = p.Stock,
                    PrecioCosto = p.PrecioCosto,
                    PrecioVTA = p.PrecioVTA
                })
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            return (lista, total);
        }

        // ============================================================
        // 🔄 REFERENCIAS POR GÉNERO
        // ============================================================

        public async Task<List<object>> ObtenerReferenciasPorGenero(int idGenero)
        {
            return await _context.Referencias
                .AsNoTracking()
                .Where(r => r.ID_Genero == idGenero && r.Activo)
                .OrderBy(r => r.DescripReferencia)
                .Select(r => new
                {
                    id = r.ID_Referencias,
                    descripcion = r.DescripReferencia
                })
                .ToListAsync<object>();
        }

        // ============================================================
        // 🔄 TALLAS POR GÉNERO
        // ============================================================

        public async Task<List<object>> ObtenerTallasPorGenero(int idGenero)
        {
            return await _context.Tallas
                .AsNoTracking()
                .Where(t => t.ID_Genero == idGenero)
                .OrderBy(t => t.DescripTalla)
                .Select(t => new
                {
                    id = t.ID_Tallas,
                    descripcion = t.DescripTalla
                })
                .ToListAsync<object>();
        }

        // ============================================================
        // 📦 REGISTRO DE PRODUCCIÓN ROBUSTO
        // ============================================================

        public async Task RegistrarProduccionAsync(
            Produccion produccion,
            List<DetalleProduccion> detalles)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (detalles == null || !detalles.Any())
                        throw new Exception("La producción debe tener al menos un detalle.");

                    // Guardar cabecera
                    _context.Producciones.Add(produccion);
                    await _context.SaveChangesAsync();

                    var productosIds = detalles
                        .Select(d => d.ID_Producto)
                        .Distinct()
                        .ToList();

                    var productos = await _context.Productos
                        .Where(p => productosIds.Contains(p.ID_Producto))
                        .AsTracking() // 🔥 ESTO ES LO QUE FALTABA
                        .ToDictionaryAsync(p => p.ID_Producto);



                    if (productos.Count != productosIds.Count)
                    {
                        throw new Exception("Uno o más productos no existen.");
                    }

                    var detallesInsertar = new List<DetalleProduccion>();

                    Console.WriteLine($"TOTAL DETALLES: {detalles.Count}");

                    foreach (var detalle in detalles)
                    {
                        Console.WriteLine($"Procesando producto: {detalle.ID_Producto} | Cantidad: {detalle.CantidadProducida}");

                        if (!productos.TryGetValue(detalle.ID_Producto, out var producto))
                            throw new Exception($"Producto {detalle.ID_Producto} no existe.");

                        if (!producto.Activo)
                            throw new Exception($"Producto {producto.Nombre} está inactivo.");

                        if (detalle.CantidadProducida <= 0)
                            throw new Exception("Cantidad inválida.");

                        if (detalle.CostoUnitario <= 0)
                            throw new Exception("Costo unitario inválido.");

                        // ================================
                        // 🔍 VALIDACIÓN DE MARGEN (NO BLOQUEANTE)
                        // ================================

                        if (producto.PrecioVTA > 0)
                        {
                            var margen = (producto.PrecioVTA - detalle.CostoUnitario) / producto.PrecioVTA;

                            if (margen < _margenAdvertencia)
                            {
                                Console.WriteLine(
                                    $"[WARN] Margen bajo → Producto: {producto.Nombre} | Margen: {Math.Round(margen * 100, 2)}%"
                                );
                            }

                            if (margen < _margenCritico)
                            {
                                Console.WriteLine(
                                    $"[CRÍTICO] Margen muy bajo → Producto: {producto.Nombre} | Margen: {Math.Round(margen * 100, 2)}%"
                                );
                            }
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[WARN] Producto sin precio de venta → {producto.Nombre}"
                            );
                        }

                        // ================================
                        // 🔒 VALIDACIÓN CRÍTICA FINAL
                        // ================================

                        if (producto.PrecioVTA > 0 && detalle.CostoUnitario >= producto.PrecioVTA)
                        {
                            throw new Exception(
                                $"El costo del producto '{producto.Nombre}' no puede ser mayor o igual al precio de venta."
                            );
                        }
                    
                    detalle.ID_Produccion = produccion.ID_Produccion;

                        detalle.PrecioVentaUnitario = producto.PrecioVTA;
                        detalle.IVA = producto.IVA_Porcentaje;

                        detalle.SubtotalCosto =
                            Math.Round(detalle.CantidadProducida * detalle.CostoUnitario, 2, MidpointRounding.AwayFromZero);

                        var baseVenta = detalle.CantidadProducida * detalle.PrecioVentaUnitario;
                        var ivaValor = (baseVenta * detalle.IVA) / 100;

                        detalle.SubtotalVenta =
                            Math.Round(baseVenta + ivaValor, 2, MidpointRounding.AwayFromZero);

                        var stockAnterior = producto.Stock;
                        var nuevoStock = stockAnterior + detalle.CantidadProducida;
                        producto.Stock = nuevoStock;

                        decimal costoActual = producto.PrecioCosto;

                        if (costoActual <= 0)
                        {
                            Console.WriteLine($"[WARN] Producto {producto.ID_Producto} sin costo previo.");
                            costoActual = detalle.CostoUnitario;
                        }
                                               
                        if (stockAnterior == 0)
                        {
                            producto.PrecioCosto = detalle.CostoUnitario;
                        }
                        else
                        {
                            var nuevoCostoPromedio =
                                ((stockAnterior * costoActual) +
                                (detalle.CantidadProducida * detalle.CostoUnitario))
                                / nuevoStock;

                            producto.PrecioCosto =
                                Math.Round(nuevoCostoPromedio, 2, MidpointRounding.AwayFromZero);
                        }

                        Console.WriteLine($"ANTES SAVE → Producto: {producto.ID_Producto} | Stock BD: {stockAnterior}");
                        Console.WriteLine($"DESPUÉS CALC → Stock NUEVO: {producto.Stock}");
                        Console.WriteLine($"COSTO NUEVO → {producto.PrecioCosto}");

                        detallesInsertar.Add(detalle);
                    }

                    // 🔥 AQUÍ ESTÁ LA CORRECCIÓN REAL
                    _context.DetalleProducciones.AddRange(detallesInsertar);

                    // 🔥 ESTE ES EL QUE DISPARA EL UPDATE DE STOCK
                    await _context.SaveChangesAsync();

                    // 🔥 CONFIRMAR TRANSACCIÓN
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR EN PRODUCCIÓN: {ex.Message}");

                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }


        /*
        // ==========================================================
        // DASHBOARD PRODUCCIÓN POR PEDIDOS ERP
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
        */

        // ============================================================
        // DASHBOARD PRODUCCIÓN POR PEDIDOS ERP
        // ============================================================

        public async Task<List<ProduccionPedidoItemVM>>
            ObtenerDashboardPedidosProduccionAsync()
        {
            var pedidos = await (
                from p in _context.Pedidos

                join c in _context.Clientes
                    on p.ID_Cliente equals c.ID_Cliente

                orderby p.ID_Pedido descending

                select new ProduccionPedidoItemVM
                {
                    ID_Pedido = p.ID_Pedido,

                    FechaPedido = p.Fecha,

                    ID_Cliente = p.ID_Cliente,

                    Cliente = c.Nombre + " " + c.Apellido,

                    Estado = p.Estado,

                    EstadoPago = p.EstadoPago,

                    TipoVenta = p.TipoVenta,

                    TotalVenta = p.Total,

                    SaldoPendiente = p.Saldo
                })
                .AsNoTracking()
                .ToListAsync();

            foreach (var pedido in pedidos)
            {
                var totalPedido =
                    await _context.DetallePedidos
                        .Where(x => x.ID_Pedido == pedido.ID_Pedido)
                        .SumAsync(x => (int?)x.Cantidad) ?? 0;

                var totalProducido =
                    await (
                        from dp in _context.DetalleProducciones

                        join det in _context.DetallePedidos
                            on dp.ID_DetallePedido equals det.ID_Detalle

                        where det.ID_Pedido == pedido.ID_Pedido

                        select dp.CantidadProducida
                    )
                    .SumAsync(x => (int?)x) ?? 0;

                pedido.TotalPedido = totalPedido;

                pedido.TotalProducido = totalProducido;

                pedido.Pendiente =
                    totalPedido - totalProducido;

                pedido.PorcentajeProduccion =
                    totalPedido == 0
                        ? 0
                        : Math.Round(
                            ((decimal)totalProducido / totalPedido) * 100,
                            2);

                pedido.EstadoProduccion =
                    totalProducido == 0
                        ? "PENDIENTE"
                        : pedido.Pendiente > 0
                            ? "EN PRODUCCIÓN"
                            : "COMPLETADO";

                pedido.UltimaProduccion =
                    await (
                        from dp in _context.DetalleProducciones

                        join det in _context.DetallePedidos
                            on dp.ID_DetallePedido equals det.ID_Detalle

                        where det.ID_Pedido == pedido.ID_Pedido

                        select (DateTime?)dp.FechaFinProduccion
                    )
                    .MaxAsync();
            }

            return pedidos;
        }


        // ============================================================
        // 📊 REPORTE POR FECHA
        // ============================================================

        public async Task<List<DetalleProduccion>> ObtenerProduccionPorFecha(DateTime fecha)
        {
            var inicio = fecha.Date;
            var fin = inicio.AddDays(1);

            return await _context.DetalleProducciones
               .AsNoTracking()
               .Where(d => d.Produccion != null &&
                    d.Produccion.FechaProduccion >= inicio &&
                    d.Produccion.FechaProduccion < fin)

                .Include(d => d.Producto!)
                .ThenInclude(p => p.Referencia)
                .OrderByDescending(d => d.Produccion.FechaProduccion)
                .ToListAsync();
        }
        public async Task<List<PendienteProduccionVM>>
          ObtenerPendientesProduccionAsync()
        {
            var data = await (
                from dp in _context.DetallePedidos

                join p in _context.Pedidos
                    on dp.ID_Pedido equals p.ID_Pedido

                join c in _context.Clientes
                    on p.ID_Cliente equals c.ID_Cliente

                join prod in _context.Productos
                    on dp.ID_Producto equals prod.ID_Producto

                join r in _context.Referencias
                    on prod.ID_Referencias equals r.ID_Referencias

                join t in _context.Tallas
                    on prod.ID_Tallas equals t.ID_Tallas

                join col in _context.Colores
                    on prod.ID_Color equals col.ID_Color

                let cantidadProducida =
                    _context.DetalleProducciones
                        .Where(x => x.ID_DetallePedido == dp.ID_Detalle)
                        .Sum(x => (int?)x.CantidadProducida) ?? 0

                let pendiente =
                    dp.Cantidad - cantidadProducida

                where pendiente > 0

                orderby p.Fecha, p.ID_Pedido

                select new PendienteProduccionVM
                {
                    ID_DetallePedido = dp.ID_Detalle,
                    ID_Pedido = p.ID_Pedido,
                    ID_Producto = prod.ID_Producto,

                    Cliente = c.Nombre + " " + c.Apellido,

                    Producto = prod.Nombre,

                    Referencia = r.DescripReferencia,

                    Talla = t.DescripTalla,

                    Color = col.Nombre,

                    CantidadPedida = dp.Cantidad,

                    CantidadProducida = cantidadProducida,

                    CantidadPendiente = pendiente,

                    EstadoProduccion =
                        pendiente == dp.Cantidad
                            ? "PENDIENTE"
                            : "EN_PROCESO"
                })
                .AsNoTracking()
                .ToListAsync();

            return data;
        }
    }
}
