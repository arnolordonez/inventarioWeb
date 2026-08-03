using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Enums;
using ClosedXML.Excel;

namespace InventarioWEB.Services
{
    public class HistorialVentasService
    {
        private readonly MovimientoVentasDbContext _context;

        public HistorialVentasService(MovimientoVentasDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // 🔥 CONSTRUYE LA CONSULTA BASE DEL HISTORIAL
        // Aplica todos los filtros sin paginación.
        // =========================================================
        private IQueryable<Pedido> ConstruirConsultaVentas(
            VentaHistorialFiltroVM filtro)
        {
            var query = _context.Pedidos
                .AsNoTracking()
                .AsQueryable();

            // Aquí se mueve TODO el código de filtros:
            // - Período
            // - EstadoPago
            // - TipoVenta
            // - EstadoDespacho
            // - Buscar
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

            return query;
        }

        // =========================================================
        // 🔥 CONVIERTE LOS PEDIDOS EN EL HISTORIAL DE VENTAS
        // Reutilizable para pantalla, Excel, PDF e impresión.
        // =========================================================
        private async Task<List<VentaHistorialVM>> MapearVentasAsync(
            List<Pedido> pedidos)
        {
            // =========================================================
            // 🔥 IDS DE PEDIDOS
            // =========================================================
            var idsPedidos = pedidos
                .Select(p => p.ID_Pedido)
                .ToList();

            // =========================================================
            // 🔥 DESPACHOS
            // =========================================================
            var despachos = await _context.Despachos
                .AsNoTracking()
                .Where(d => idsPedidos.Contains(d.ID_Pedido))
                .ToListAsync();

            // =========================================================
            // 🔥 DETALLES
            // =========================================================
            var detalles = await _context.DetallePedidos
                .AsNoTracking()
                .Where(d => idsPedidos.Contains(d.ID_Pedido))
                .ToListAsync();

            // =========================================================
            // 🔥 CLIENTES
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
            // 🔥 DESPACHOS POR PEDIDO
            // =========================================================
            var despachosPorPedido = despachos
                .GroupBy(d => d.ID_Pedido)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.FechaRegistro).First());

            // =========================================================
            // 🔥 DETALLES POR PEDIDO
            // =========================================================
            var detallesPorPedido = detalles
                .ToLookup(d => d.ID_Pedido);

            // =========================================================
            // 🔥 MAPEO
            // =========================================================
            return pedidos.Select(p =>
            {
                despachosPorPedido.TryGetValue(p.ID_Pedido, out var despacho);

                clientes.TryGetValue(p.ID_Cliente, out var cliente);

                var detalle = detallesPorPedido[p.ID_Pedido].ToList();

                return new VentaHistorialVM
                {
                    ID_Pedido = p.ID_Pedido,

                    Cliente = cliente != null
                        ? $"{cliente.Nombre} {cliente.Apellido}"
                        : p.ID_Cliente.ToString(),

                    

                    Fecha = p.Fecha,


                    TipoVenta = p.TipoVenta,

                    EstadoPedido = (p.Estado ?? "").Trim().ToUpper(),
                    EstadoPago = (p.EstadoPago ?? "").Trim().ToUpper(),
                    EstadoDespacho = (p.Estado ?? "").Trim().ToUpper(),

                    Subtotal = p.Total,
                    TotalIVA = p.TotalIVA,
                    TotalVenta = p.TotalVenta,
                    Saldo = p.Saldo,
                    TotalAbonado = p.TotalVenta - p.Saldo,

                    ID_Despacho = despacho?.ID_Despacho,

                    TotalProductos = detalle.Count,
                    TotalUnidades = detalle.Sum(x => x.Cantidad)
                };

            }).ToList();
        }


        public async Task<HistorialVentasResultadoVM> ObtenerVentasAsync(VentaHistorialFiltroVM filtro)
        {
            // =========================================================
            // 🔥 BASE PEDIDOS
            // =========================================================
            var query = ConstruirConsultaVentas(filtro);

           
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
            var resultado = await MapearVentasAsync(pedidos);
                        
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

        /// <summary>
        /// Obtiene el historial de ventas para procesos de exportación
        /// (Excel, PDF e impresión), aplicando los mismos filtros del
        /// historial pero sin paginación.
        /// </summary>
        /// <param name="filtro">
        /// Filtros de consulta del historial de ventas.
        /// </param>
        /// <returns>
        /// Lista completa de ventas que cumplen los filtros indicados.
        /// </returns>
        // =========================================================
        // 🔥 OBTIENE TODAS LAS VENTAS FILTRADAS (SIN PAGINACIÓN)
        // Utilizado para Exportar Excel, PDF e Imprimir.
        // =========================================================
        public async Task<List<VentaHistorialVM>> ObtenerVentasExportacionAsync(
            VentaHistorialFiltroVM filtro)
        {
            // =========================================================
            // 🔥 CONSULTA BASE
            // =========================================================
            var query = ConstruirConsultaVentas(filtro);

            // =========================================================
            // 🔥 PEDIDOS FILTRADOS
            // =========================================================
            var pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.ID_Pedido)
                .ToListAsync();

            // =========================================================
            // 🔥 MAPEO
            // =========================================================
            return await MapearVentasAsync(pedidos);
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
                // =====================================================
                // DATOS GENERALES
                // =====================================================

                ID_Pedido = pedido.ID_Pedido,

                Cliente = cliente != null
                ? $"{cliente.Nombre} {cliente.Apellido}"
                : pedido.ID_Cliente.ToString(),

                CorreoCliente = cliente?.Correo ?? string.Empty,

                Fecha = pedido.Fecha,

                TipoVenta = pedido.TipoVenta,

                // =====================================================
                // INFORMACIÓN FINANCIERA
                // =====================================================

                // Base gravable (sin IVA)
                Total = pedido.Total,

                // IVA total registrado en el pedido
                TotalIVA = pedido.TotalIVA,      // <-- Cambia por TotalIVA si así se llama en tu entidad

                // Total de la venta (Base + IVA)
                TotalVenta = pedido.TotalVenta,

                // Total abonado por el cliente
                TotalAbonado = abonosVM.Any()
                    ? abonosVM.Sum(a => a.Monto)
                    : pedido.TotalVenta - pedido.Saldo,

                // =====================================================
                // ESTADOS DEL PROCESO
                // =====================================================

                EstadoPedido = pedido.Estado,

                EstadoPago = pedido.EstadoPago,

                EstadoDespacho = despacho?.Estado.ToString() ?? "NO DESPACHADO",

                TipoDespacho = despacho?.Tipo.ToString() ?? "SIN DESPACHO",

                ID_Despacho = despacho?.ID_Despacho,

                // =====================================================
                // DETALLE DE LA VENTA
                // =====================================================

                Productos = productosVM,

                Abonos = abonosVM
            };

        }
    }
}