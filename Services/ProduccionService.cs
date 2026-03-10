using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;


namespace InventarioWEB.Services
{
    public class ProduccionService
    {
        private readonly MovimientoVentasDbContext _context;

        public ProduccionService(MovimientoVentasDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // 🔍 BÚSQUEDA OPTIMIZADA DE PRODUCTOS PARA PRODUCCIÓN
        // - Soporta búsqueda exacta por código (PK)
        // - Soporta filtros combinables
        // - Incluye paginación
        // - Evita listar todos los registros
        // - No usa Include (optimización de rendimiento)
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
            // =========================================================
            // 1️⃣ BÚSQUEDA POR PK (FAST PATH)
            // =========================================================
            if (idProducto.HasValue && idProducto.Value > 0)
            {
                var pkQuery =
                    from p in _context.Productos.AsNoTracking()
                    join r in _context.Referencias on p.ID_Referencias equals r.ID_Referencias
                    join t in _context.Tallas on p.ID_Tallas equals t.ID_Tallas
                    join te in _context.Telas on p.ID_Telas equals te.ID_Telas
                    join c in _context.Colores on p.ID_Color equals c.ID_Color
                    where p.ID_Producto == idProducto.Value && p.Activo
                    select new ProductoProduccionDTO
                    {
                        ID_Producto = p.ID_Producto,
                        Nombre = p.Nombre,
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

            // =========================================================
            // 2️⃣ VALIDACIÓN ANTI FULL SCAN
            // =========================================================
            if (!idGenero.HasValue &&
                !idReferencia.HasValue &&
                !idTalla.HasValue &&
                !idTela.HasValue &&
                !idColor.HasValue)
            {
                return (new List<ProductoProduccionDTO>(), 0);
            }

            // =========================================================
            // 3️⃣ QUERY BASE SOBRE ENTIDAD (NO DTO)
            // =========================================================
            var baseQuery = _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo);

            // =========================================================
            // 4️⃣ FILTROS SOBRE PRODUCTOS (CORRECTO)
            // =========================================================
            if (idGenero.HasValue && idGenero.Value > 0)
                baseQuery = baseQuery.Where(p => p.ID_Genero == idGenero.Value);

            if (idReferencia.HasValue && idReferencia.Value > 0)
                baseQuery = baseQuery.Where(p => p.ID_Referencias == idReferencia.Value);

            if (idTalla.HasValue && idTalla.Value > 0)
                baseQuery = baseQuery.Where(p => p.ID_Tallas == idTalla.Value);

            if (idTela.HasValue && idTela.Value > 0)
                baseQuery = baseQuery.Where(p => p.ID_Telas == idTela.Value);

            if (idColor.HasValue && idColor.Value > 0)
                baseQuery = baseQuery.Where(p => p.ID_Color == idColor.Value);

            // =========================================================
            // 5️⃣ TOTAL (ANTES DE PAGINAR)
            // =========================================================
            var total = await baseQuery.CountAsync();

            if (total == 0)
                return (new List<ProductoProduccionDTO>(), 0);

            // =========================================================
            // 6️⃣ JOIN + PAGINACIÓN + PROYECCIÓN FINAL
            // =========================================================
            var lista =
                await (
                    from p in baseQuery
                    join r in _context.Referencias on p.ID_Referencias equals r.ID_Referencias
                    join t in _context.Tallas on p.ID_Tallas equals t.ID_Tallas
                    join te in _context.Telas on p.ID_Telas equals te.ID_Telas
                    join c in _context.Colores on p.ID_Color equals c.ID_Color
                    orderby p.ID_Referencias, p.ID_Tallas, p.ID_Telas, p.ID_Color
                    select new ProductoProduccionDTO
                    {
                        ID_Producto = p.ID_Producto,
                        Nombre = p.Nombre,
                        Referencia = r.DescripReferencia,
                        Talla = t.DescripTalla,
                        Tela = te.DescripTela,
                        Color = c.Nombre,
                        Stock = p.Stock,
                        PrecioCosto = p.PrecioCosto,
                        PrecioVTA = p.PrecioVTA
                    }
                )
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

                    _context.Producciones.Add(produccion);
                    await _context.SaveChangesAsync();

                    var productosIds = detalles
                        .Select(d => d.ID_Producto)
                        .Distinct()
                        .ToList();

                    var productos = await _context.Productos
                        .Where(p => productosIds.Contains(p.ID_Producto))
                        .ToDictionaryAsync(p => p.ID_Producto);

                    foreach (var detalle in detalles)
                    {
                        if (!productos.TryGetValue(detalle.ID_Producto, out var producto))
                            throw new Exception($"Producto {detalle.ID_Producto} no existe.");

                        if (detalle.Cantidad <= 0)
                            throw new Exception("Cantidad inválida.");

                        if (detalle.CostoUnitario <= 0)
                            throw new Exception("Costo unitario inválido.");

                        detalle.ID_Produccion = produccion.ID_Produccion;

                        // SNAPSHOT comercial
                        detalle.PrecioVentaUnitario = producto.PrecioVTA;
                        detalle.IVA = producto.IVA_Porcentaje;

                        // Cálculo costo
                        detalle.SubtotalCosto = detalle.Cantidad * detalle.CostoUnitario;

                        var baseVenta = detalle.Cantidad * detalle.PrecioVentaUnitario;
                        var ivaValor = (baseVenta * detalle.IVA) / 100;
                        detalle.SubtotalVenta = baseVenta + ivaValor;

                        // 🔥 PROMEDIO PONDERADO CORRECTO
                        var nuevoStock = producto.Stock + detalle.Cantidad;

                        if (nuevoStock <= 0)
                            throw new Exception("Stock final inválido.");

                        var nuevoCostoPromedio =
                            ((producto.Stock * producto.PrecioCosto)
                             + (detalle.Cantidad * detalle.CostoUnitario))
                             / nuevoStock;

                        producto.Stock = nuevoStock;
                        producto.PrecioCosto = Math.Round(nuevoCostoPromedio, 2);

                        _context.DetalleProducciones.Add(detalle);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
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
                .Where(d => d.Produccion.FechaProduccion >= inicio &&
                            d.Produccion.FechaProduccion < fin)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Referencia)
                .OrderByDescending(d => d.Produccion.FechaProduccion)
                .ToListAsync();
        }
    }
}