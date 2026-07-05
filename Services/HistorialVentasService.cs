using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Enums;

namespace InventarioWEB.Services
{
    public class HistorialVentasService
    {
        private readonly MovimientoVentasDbContext _context;

        public HistorialVentasService(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        public async Task<HistorialVentasResultadoVM> ObtenerVentasAsync(VentaHistorialFiltroVM filtro)
        {
            // =========================================================
            // 🔥 BASE PEDIDOS
            // =========================================================
            var query = _context.Pedidos
                .AsNoTracking()
                .AsQueryable();

            // =========================================================
            // 🔥 FILTRO POR PERÍODO
            // =========================================================
            var hoy = DateTime.Today;

            switch (filtro.Periodo)
            {
                // =====================================================
                // HOY
                // =====================================================
                case PeriodoHistorial.Hoy:

                    query = query.Where(p =>
                        p.Fecha >= hoy &&
                        p.Fecha < hoy.AddDays(1));

                    break;

                // =====================================================
                // ESTA SEMANA (Lunes - Domingo)
                // =====================================================
                case PeriodoHistorial.Semana:

                    int diasDesdeLunes = ((int)hoy.DayOfWeek + 6) % 7;

                    var inicioSemana = hoy.AddDays(-diasDesdeLunes);

                    var finSemana = inicioSemana.AddDays(7);

                    query = query.Where(p =>
                        p.Fecha >= inicioSemana &&
                        p.Fecha < finSemana);

                    break;

                // =====================================================
                // MES ACTUAL
                // =====================================================
                case PeriodoHistorial.Mes:

                    query = query.Where(p =>
                        p.Fecha.Month == hoy.Month &&
                        p.Fecha.Year == hoy.Year);

                    break;

                // =====================================================
                // MES ANTERIOR
                // =====================================================
                case PeriodoHistorial.MesAnterior:

                    var mesAnterior = hoy.AddMonths(-1);

                    query = query.Where(p =>
                        p.Fecha.Month == mesAnterior.Month &&
                        p.Fecha.Year == mesAnterior.Year);

                    break;

                // =====================================================
                // POR MES
                // =====================================================
                case PeriodoHistorial.PorMes:

                    if (filtro.Mes.HasValue && filtro.AnioMes.HasValue)
                    {
                        query = query.Where(p =>
                            p.Fecha.Month == filtro.Mes.Value &&
                            p.Fecha.Year == filtro.AnioMes.Value);
                    }

                    break;

                // =====================================================
                // POR FECHA
                // =====================================================
                case PeriodoHistorial.PorFecha:

                    // Si el usuario invierte las fechas, se corrigen automáticamente.
                    if (filtro.FechaDesde.HasValue &&
                        filtro.FechaHasta.HasValue &&
                        filtro.FechaDesde > filtro.FechaHasta)
                    {
                        (filtro.FechaDesde, filtro.FechaHasta) =
                            (filtro.FechaHasta, filtro.FechaDesde);
                    }

                    if (filtro.FechaDesde.HasValue)
                    {
                        query = query.Where(p =>
                            p.Fecha >= filtro.FechaDesde.Value.Date);
                    }

                    if (filtro.FechaHasta.HasValue)
                    {
                        var fechaHasta = filtro.FechaHasta.Value.Date.AddDays(1);

                        query = query.Where(p =>
                            p.Fecha < fechaHasta);
                    }

                    break;

                // =====================================================
                // POR AÑO
                // =====================================================
                case PeriodoHistorial.PorAnio:

                    if (filtro.Anio.HasValue)
                    {
                        query = query.Where(p =>
                            p.Fecha.Year == filtro.Anio.Value);
                    }

                    break;

                // =====================================================
                // SIN FILTRO ADICIONAL
                // =====================================================
                default:
                    break;
            }
            // =========================================================
            // 🔥 FILTRO POR ESTADO DE PAGO
            // =========================================================
            if (!string.IsNullOrWhiteSpace(filtro.EstadoPago) &&
                !filtro.EstadoPago.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.EstadoPago == filtro.EstadoPago);
            }

            // =========================================================
            // 🔥 FILTRO POR TIPO DE VENTA
            // =========================================================
            if (!string.IsNullOrWhiteSpace(filtro.TipoVenta) &&
                !filtro.TipoVenta.Equals("Todas", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.TipoVenta == filtro.TipoVenta);
            }

            // =========================================================
            // 🔥 FILTRO POR ESTADO DE DESPACHO
            // =========================================================
            if (!string.IsNullOrWhiteSpace(filtro.EstadoDespacho) &&
                !filtro.EstadoDespacho.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Estado == filtro.EstadoDespacho);
            }



            // =========================================================
            // 🔥 FILTRO GENERAL (PEDIDO / CLIENTE)
            // =========================================================
            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var texto = filtro.Buscar.Trim().ToUpper();
                bool esNumero = int.TryParse(texto, out int numero);

                query =
                    from p in query
                    join c in _context.Clientes.AsNoTracking()
                        on p.ID_Cliente equals c.ID_Cliente
                    where

                        // ===========================
                        // PEDIDO (coincidencia exacta)
                        // ===========================
                        (esNumero && p.ID_Pedido == numero)

                        ||

                        // ===========================
                        // CÉDULA (coincidencia exacta)
                        // ===========================
                        (esNumero && c.ID_Cliente == numero)

                        ||

                        // ===========================
                        // NOMBRE
                        // ===========================
                        c.Nombre.ToUpper().Contains(texto)

                        ||

                        // ===========================
                        // APELLIDO
                        // ===========================
                        c.Apellido.ToUpper().Contains(texto)

                        ||

                        // ===========================
                        // NOMBRE COMPLETO
                        // ===========================
                        (c.Nombre + " " + c.Apellido)
                            .ToUpper()
                            .Contains(texto)

                    select p;
            }

            // =========================================================
            // 🔥 INDICADORES (ANTES DE PAGINAR)
            // =========================================================

            var totalRegistros = await query.CountAsync();

            var totalPagadas = await query.CountAsync(p =>
                p.EstadoPago == "PAGADO");

            var totalAbonadas = await query.CountAsync(p =>
                p.EstadoPago == "ABONADO");

            var totalSaldoPendiente = await query.SumAsync(p =>
                (decimal?)p.Saldo) ?? 0;

            // =========================================================
            // 🔥 PAGINACIÓN
            // =========================================================

            if (filtro.Pagina < 1)
                filtro.Pagina = 1;

            if (filtro.RegistrosPorPagina <= 0)
                filtro.RegistrosPorPagina = 20;

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)filtro.RegistrosPorPagina);

            if (totalPaginas <= 0)
                totalPaginas = 1;

            if (filtro.Pagina > totalPaginas)
                filtro.Pagina = totalPaginas;

            // =========================================================
            // 🔥 PEDIDOS FILTRADOS (PAGINADOS)
            // Orden:
            // 1. Fecha descendente (más recientes primero)
            // 2. ID del Pedido descendente (si la fecha es igual)
            // =========================================================

            var pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.ID_Pedido)
                .Skip((filtro.Pagina - 1) * filtro.RegistrosPorPagina)
                .Take(filtro.RegistrosPorPagina)
                .ToListAsync();

            // =========================================================
            // 🔥 IDS DE PEDIDOS
            // =========================================================

            var idsPedidos = pedidos
                .Select(p => p.ID_Pedido)
                .ToList();
            // =========================================================
            // 🔥 DESPACHOS ÚNICAMENTE DE ESOS PEDIDOS
            // =========================================================
            var despachos = await _context.Despachos
                .AsNoTracking()
                .Where(d => idsPedidos.Contains(d.ID_Pedido))
                .ToListAsync();

            // =========================================================
            // 🔥 DETALLES ÚNICAMENTE DE ESOS PEDIDOS
            // =========================================================
            var detalles = await _context.DetallePedidos
                .AsNoTracking()
                .Where(d => idsPedidos.Contains(d.ID_Pedido))
                .ToListAsync();

            // =========================================================
            // 🔥 CLIENTES DE LOS PEDIDOS
            // =========================================================
            var idsClientes = pedidos
                .Select(p => p.ID_Cliente)
                .Distinct()
                .ToList();

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c => idsClientes.Contains(c.ID_Cliente))
                .ToDictionaryAsync(c => c.ID_Cliente);

            // =========================================================
            // 🔥 ÚLTIMO DESPACHO POR PEDIDO
            // =========================================================
            var despachosPorPedido = despachos
                .GroupBy(d => d.ID_Pedido)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.FechaRegistro).First()
                );

            // =========================================================
            // 🔥 DETALLES POR PEDIDO (LOOKUP O(1))
            // =========================================================
            var detallesPorPedido = detalles
                .ToLookup(d => d.ID_Pedido);

            // =========================================================
            // 🔥 MAPEO EN MEMORIA (ERP CORRECTO - OPTIMIZADO Y LIMPIO)
            // =========================================================
            var resultado = pedidos.Select(p =>
            {
                // =========================
                // DESPACHO (O(1))
                // =========================
                despachosPorPedido.TryGetValue(p.ID_Pedido, out var despacho);

                // =========================
                // DETALLES (SAFE LOOKUP)
                // =========================
                var detalle = detallesPorPedido[p.ID_Pedido].ToList();

                // =========================
                // CLIENTE
                // =========================
                clientes.TryGetValue(p.ID_Cliente, out var cliente);

                return new VentaHistorialVM
                {
                    // =========================
                    // IDENTIFICADOR
                    // =========================
                    ID_Pedido = p.ID_Pedido,

                    // =========================
                    // COMERCIAL
                    // =========================
                    Cliente = cliente != null
                        ? $"{cliente.Nombre} {cliente.Apellido}"
                        : p.ID_Cliente.ToString(),

                    Fecha = p.Fecha,
                    TipoVenta = p.TipoVenta,

                    // =========================
                    // ESTADOS
                    // =========================
                    EstadoPedido = (p.Estado ?? "").Trim().ToUpper(),

                    EstadoPago = (p.EstadoPago ?? "").Trim().ToUpper(),

                    EstadoDespacho = (p.Estado ?? "").Trim().ToUpper(),

                    // =========================
                    // FINANCIERO
                    // =========================
                    Subtotal = p.Total,
                    TotalIVA = p.TotalIVA,
                    TotalVenta = p.TotalVenta,
                    Saldo = p.Saldo,
                    TotalAbonado = p.TotalVenta - p.Saldo,

                    // =========================
                    // FACTURA
                    // =========================
                    ID_Despacho = despacho?.ID_Despacho,

                    // =========================
                    // PRODUCTOS
                    // =========================
                    TotalProductos = detalle.Count,
                    TotalUnidades = detalle.Sum(x => x.Cantidad)
                };
            })
            .ToList();
            // =========================================================
            // 🔥 RESULTADO DEL HISTORIAL
            // =========================================================
            return new HistorialVentasResultadoVM
            {
                Ventas = resultado,

                TotalRegistros = totalRegistros,

                TotalPagadas = totalPagadas,

                TotalAbonadas = totalAbonadas,

                TotalSaldoPendiente = totalSaldoPendiente,

                PaginaActual = filtro.Pagina,

                TotalPaginas = totalPaginas
            };

        }

        // =========================================================
        // 🔥 DETALLE COMPLETO DE UNA VENTA (ERP PRODUCCIÓN)
        // =========================================================
        public async Task<VentaDetalleVM?> ObtenerDetalleVentaAsync(int idPedido)
        {
            // =====================================================
            // 🔹 PEDIDO
            // =====================================================
            var pedido = await _context.Pedidos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID_Pedido == idPedido);

            if (pedido == null)
                return null;

            // =====================================================
            // 🔹 CLIENTE
            // =====================================================
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID_Cliente == pedido.ID_Cliente);

            // =====================================================
            // 🔹 DESPACHO
            // =====================================================
            var despacho = await _context.Despachos
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ID_Pedido == idPedido);


            // =====================================================
            // 🔹 ABONOS
            // =====================================================
            var abonos = await _context.Abonos
                .AsNoTracking()
                .Include(a => a.MetodoPago)
                .Where(a => a.ID_Pedido == idPedido && a.Activo)
                .OrderBy(a => a.Fecha_Abono)
                .ToListAsync();


            // =====================================================
            // 🔹 DETALLES
            // =====================================================
            var detalles = await _context.DetallePedidos
                .AsNoTracking()
                .Where(d => d.ID_Pedido == idPedido)
                .ToListAsync();

            // =====================================================
            // 🔹 PRODUCTOS
            // =====================================================
            var productosIds = detalles
                .Select(d => d.ID_Producto)
                .Distinct()
                .ToList();

            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => productosIds.Contains(p.ID_Producto))
                .ToDictionaryAsync(p => p.ID_Producto);

            // =====================================================
            // 🔹 COLORES (TABLA REAL)
            // =====================================================
            var coloresIds = productos.Values
                .Select(p => p.ID_Color)
                .Distinct()
                .ToList();

            var colores = await _context.Colores
                .AsNoTracking()
                .Where(c => coloresIds.Contains(c.ID_Color))
                .ToDictionaryAsync(c => c.ID_Color);

            // =====================================================
            // 🔹 TALLAS
            // =====================================================
            var tallasIds = productos.Values
                .Select(p => p.ID_Tallas)
                .Distinct()
                .ToList();

            var tallas = await _context.Tallas
                .AsNoTracking()
                .Where(t => tallasIds.Contains(t.ID_Tallas))
                .ToDictionaryAsync(t => t.ID_Tallas);

            // =====================================================
            // 🔹 MAPEO PRODUCTOS VM
            // =====================================================
            var productosVM = detalles.Select(d =>
            {
                productos.TryGetValue(d.ID_Producto, out var prod);

                string colorNombre = "";
                string tallaNombre = "";

                if (prod != null)
                {
                    if (colores.TryGetValue(prod.ID_Color, out var col))
                        colorNombre = col.Nombre; // ajusta si tu campo tiene otro nombre

                    if (tallas.TryGetValue(prod.ID_Tallas, out var tal))
                        tallaNombre = tal.DescripTalla;
                }

                return new DetalleProductoVM
                {
                    ID_Producto = d.ID_Producto,
                    Producto = prod?.Nombre ?? "N/A",
                    Color = colorNombre,
                    Talla = tallaNombre,
                    Cantidad = d.Cantidad,
                    PrecioVenta = d.PrecioVenta,
                    Subtotal = d.Subtotal
                };
            }).ToList();


            // =====================================================
            // 🔹 MAPEO ABONOS VM
            // =====================================================
            var abonosVM = abonos.Select(a => new AbonoDetalleVM
            {
                Fecha_Abono = a.Fecha_Abono,
                Monto = a.Monto,
                MetodoPago = a.MetodoPago.Nombre,   // Ajusta esta propiedad si en tu modelo tiene otro nombre
                NumeroRecibo = a.NumeroRecibo ?? ""
            }).ToList();

            // =====================================================
            // 🔹 VM FINAL
            // =====================================================
            return new VentaDetalleVM
            {
                ID_Pedido = pedido.ID_Pedido,
                Cliente = cliente != null
                    ? $"{cliente.Nombre} {cliente.Apellido}"
                    : pedido.ID_Cliente.ToString(),

                Fecha = pedido.Fecha,

                TotalVenta = pedido.TotalVenta,
                TotalAbonado = abonosVM.Any()
                ? abonosVM.Sum(a => a.Monto)
                : pedido.TotalVenta - pedido.Saldo,
               
                TipoVenta = pedido.TipoVenta,

                EstadoPedido = pedido.Estado,
                EstadoPago = pedido.EstadoPago,
                EstadoDespacho = despacho?.Estado.ToString() ?? "NO DESPACHADO",
                ID_Despacho = despacho?.ID_Despacho,
                               
                Productos = productosVM,
                Abonos = abonosVM
            };
        }
    }
}