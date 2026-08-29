using System;
using System.Threading.Tasks;
using InventarioWEB.Data;
using InventarioWEB.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using InventarioWEB.ViewModels;
using InventarioWEB.DTOs;
using InventarioWEB.Models.Enums;
using InventarioWEB.Services.Mapping;
using Microsoft.Extensions.Logging;
using InventarioWEB.Constants;

namespace InventarioWEB.Services
{
    public class HistorialInventarioService
    {
        private readonly MovimientoVentasDbContext _context;
        private readonly ILogger<HistorialInventarioService> _logger;

        public HistorialInventarioService(
            MovimientoVentasDbContext context,
            ILogger<HistorialInventarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

       
        // =========================================================
        // 🔥 REGISTRO BASE (REGLAS DE NEGOCIO + VALIDACIÓN CENTRAL)
        // =========================================================
        public async Task RegistrarMovimientoAsync(HistorialInventario movimiento)
        {
            if (movimiento == null)
                throw new ArgumentNullException(nameof(movimiento));

            if (string.IsNullOrWhiteSpace(movimiento.TipoMovimiento))
                throw new ArgumentException("TipoMovimiento es obligatorio");

            if (movimiento.IdProducto <= 0)
                throw new ArgumentException("IdProducto es obligatorio");

            if (movimiento.UsuarioId <= 0)
                throw new ArgumentException("UsuarioId inválido");

            if (string.IsNullOrWhiteSpace(movimiento.UsuarioNombre))
                throw new ArgumentException("UsuarioNombre es obligatorio");

            if (string.IsNullOrWhiteSpace(movimiento.DocumentoReferencia))
                movimiento.DocumentoReferencia = "SIN-DOC";

            if (movimiento.Cantidad == 0)
                throw new ArgumentException("Cantidad no puede ser 0");

            if (movimiento.StockActual < 0)
                throw new Exception("Stock no puede ser negativo");

            if (movimiento.StockActual != movimiento.StockAnterior + movimiento.Cantidad)
                throw new Exception("Inconsistencia en cálculo de stock");

            // =========================================================
            // RESPETAR LA FECHA DEL MOVIMIENTO
            // Solo si no fue enviada se usa la fecha actual.
            // =========================================================
            if (movimiento.FechaRegistro == default(DateTime))
            {
                movimiento.FechaRegistro = DateTime.Now;
            }

            await _context.HistorialInventario.AddAsync(movimiento);
            await _context.SaveChangesAsync();
        }

        // =========================================================
        // 🔻 REGISTRO DE DESPACHO (SALIDA REAL DE INVENTARIO)
        // =========================================================
        public async Task RegistrarDespachoAsync(
            Producto producto,
            int cantidad,
            int stockAnterior,
            int stockActual,
            int usuarioId,
            string usuarioNombre,
            int ventaId,
            int despachoId,
            string cliente,
            string documentoReferencia)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            var movimiento = CrearMovimientoBase(
                tipoMovimiento: "VENTA_DESPACHO",
                documentoReferencia: documentoReferencia,

                producto: producto,   // ✅ ESTE ES EL CAMBIO CLAVE

                cantidad: -Math.Abs(cantidad),
                stockAnterior: stockAnterior,
                stockActual: stockActual,

                usuarioId: usuarioId,
                usuarioNombre: usuarioNombre,

                ventaId: ventaId,
                despachoId: despachoId,

                cliente: cliente,
                observaciones: $"Despacho asociado a venta {ventaId}"
            );

            await RegistrarMovimientoAsync(movimiento);
        }
        // =========================================================
        // 🧾 REGISTRO DE VENTA (NO DESCARGA STOCK)
        // =========================================================
        public async Task RegistrarVentaAsync(
            Producto producto,
            int cantidad,
            int stockActual,
            int usuarioId,
            string usuarioNombre,
            int ventaId,
            string cliente,
            string documentoReferencia)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            var movimiento = CrearMovimientoBase(
                tipoMovimiento: "VENTA",
                documentoReferencia: documentoReferencia,

                producto: producto,

                cantidad: -Math.Abs(cantidad),

                stockAnterior: stockActual,
                stockActual: stockActual,

                usuarioId: usuarioId,
                usuarioNombre: usuarioNombre,

                ventaId: ventaId,
                cliente: cliente,

                observaciones: "Venta registrada pendiente de despacho"
            );

            await RegistrarMovimientoAsync(movimiento);
        }

        // =========================================================
        // 📦 ENTRADAS (PRODUCCIÓN / AJUSTES / REPROCESO)
        // =========================================================
        public async Task RegistrarEntradaAsync(
            Producto producto,
            int cantidad,
            int stockAnterior,
            int stockActual,
            int usuarioId,
            string usuarioNombre,
            string tipoEntrada,
            string documentoReferencia)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            var movimiento = CrearMovimientoBase(
                tipoMovimiento: tipoEntrada,
                documentoReferencia: documentoReferencia,

                producto: producto,

                cantidad: cantidad,

                stockAnterior: stockAnterior,
                stockActual: stockActual,

                usuarioId: usuarioId,
                usuarioNombre: usuarioNombre,

                observaciones: $"Entrada de inventario tipo {tipoEntrada}"
            );

            await RegistrarMovimientoAsync(movimiento);
        }
        // =========================================================
        // 🧠 FACTORY CENTRAL DE MOVIMIENTOS (ÚNICA FUENTE DE VERDAD)
        // =========================================================
        // =========================================================
        // 🧠 FACTORY CENTRAL (VERSIÓN ERP ROBUSTA)
        // =========================================================
        private HistorialInventario CrearMovimientoBase(
            string tipoMovimiento,
            string documentoReferencia,
            Producto producto,
            int cantidad,
            int stockAnterior,
            int stockActual,
            int usuarioId,
            string usuarioNombre,
            int? ventaId = null,
            int? despachoId = null,
            string? cliente = null,
            string? observaciones = null)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            return new HistorialInventario
            {
                // =====================================================
                // AUDITORÍA
                // =====================================================
                FechaRegistro = DateTime.Now,
                TipoMovimiento = tipoMovimiento,
                DocumentoReferencia = documentoReferencia,

                // =====================================================
                // IDENTIDAD
                // =====================================================
                IdProducto = producto.ID_Producto,
                IdGenero = producto.ID_Genero,

                NombreProducto = producto.Nombre,

                             Referencia =
                    _context.Referencias
                        .Where(r => r.ID_Referencias == producto.ID_Referencias)
                        .Select(r => r.DescripReferencia)
                        .FirstOrDefault() ?? "SIN_REFERENCIA",

                                Talla =
                    _context.Tallas
                        .Where(t => t.ID_Tallas == producto.ID_Tallas)
                        .Select(t => t.DescripTalla)
                        .FirstOrDefault() ?? "SIN_TALLA",

                                Color =
                    _context.Colores
                        .Where(c => c.ID_Color == producto.ID_Color)
                        .Select(c => c.Nombre)
                        .FirstOrDefault() ?? "SIN_COLOR",

                                Tela =
                    _context.Telas
                        .Where(t => t.ID_Telas == producto.ID_Telas)
                        .Select(t => t.DescripTela)
                        .FirstOrDefault() ?? "SIN_TELA",
                // =====================================================
                // MOVIMIENTO
                // =====================================================
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockActual = stockActual,

                // =====================================================
                // USUARIO
                // =====================================================
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,

                // =====================================================
                // RELACIONES
                // =====================================================
                VentaId = ventaId,
                DespachoId = despachoId,
                Cliente = cliente,

                // =====================================================
                // OBSERVACIONES
                // =====================================================
                Observaciones = observaciones
            };
        }



        // =========================================================
        // 🔍 VALIDACIÓN DE CONSISTENCIA DEL HISTORIAL INVENTARIO
        // =========================================================
        public async Task<List<HistorialInconsistenciaVM>> ValidarConsistenciaHistorialInventario()
        {
            var resultados = await _context.HistorialInventario
                .AsNoTracking()
                .Select(h => new HistorialInconsistenciaVM
                {
                    Id = h.Id,
                    Fecha = h.FechaRegistro,
                    TipoMovimiento = h.TipoMovimiento,
                    Documento = h.DocumentoReferencia,

                    IdProducto = h.IdProducto,
                    NombreProducto = h.NombreProducto,

                    Referencia = h.Referencia,
                    Talla = h.Talla,
                    Color = h.Color,
                    Tela = h.Tela,

                    Cantidad = h.Cantidad,
                    StockAnterior = h.StockAnterior,
                    StockActual = h.StockActual,

                    Usuario = h.UsuarioNombre,

                    VentaId = h.VentaId,
                    DespachoId = h.DespachoId,
                    
                    Cliente = h.Cliente ?? "SIN CLIENTE",
                    Observaciones = h.Observaciones ?? "SIN OBSERVACIONES",
                    // ===========================
                    // FLAGS DE VALIDACIÓN
                    // ===========================
                    SinProducto = false,
                    SinReferencia = false,
                    SinTalla = false,
                    SinColor = false,
                    SinTela = false,
                    MovimientoInvalido = false
                })
                .ToListAsync();

            foreach (var item in resultados)
            {
                // =====================================================
                // 🔴 VALIDACIÓN CATÁLOGO
                // =====================================================

                if (string.IsNullOrWhiteSpace(item.NombreProducto))
                    item.SinProducto = true;

                if (string.IsNullOrWhiteSpace(item.Referencia) || item.Referencia == "SIN_REFERENCIA")
                    item.SinReferencia = true;

                if (string.IsNullOrWhiteSpace(item.Talla) || item.Talla == "SIN_TALLA")
                    item.SinTalla = true;

                if (string.IsNullOrWhiteSpace(item.Color) || item.Color == "SIN_COLOR")
                    item.SinColor = true;

                if (string.IsNullOrWhiteSpace(item.Tela) || item.Tela == "SIN_TELA")
                    item.SinTela = true;

                // =====================================================
                // 🔴 VALIDACIÓN LÓGICA DE MOVIMIENTOS
                // =====================================================

                if (item.TipoMovimiento == "VENTA_DESPACHO")
                {
                    if (item.Cantidad >= 0)
                        item.MovimientoInvalido = true; // despacho debería ser negativo
                }

                if (item.TipoMovimiento == "PRODUCCION")
                {
                    if (item.Cantidad <= 0)
                        item.MovimientoInvalido = true; // producción debe ser entrada positiva
                }
            }

            return resultados
                .Where(x =>
                    x.SinProducto ||
                    x.SinReferencia ||
                    x.SinTalla ||
                    x.SinColor ||
                    x.SinTela ||
                    x.MovimientoInvalido)
                .ToList();
        }
        
        /// =========================================================
        /// 📊 KARDEX DE INVENTARIO (MOTOR CENTRAL DEL SISTEMA)
        /// =========================================================
        /// 🔹 Consulta, transforma y calcula el Kardex completo.
        /// 🔹 Aplica filtros dinámicos.
        /// 🔹 Genera la información para tabla, tarjetas y gráfica.
        /// 🔹 Fuente oficial de información del Kardex.
        /// =========================================================
        public async Task<KardexResultViewModel> ObtenerKardexCompletoAsync(KardexFilterDto filter)
        {
            // =========================================================
            // 1. CONSULTA BASE
            // =========================================================
            // Se crea la consulta principal sobre HistorialInventario.
            // Todos los filtros se aplican sobre este IQueryable.
            // =========================================================

            var query = _context.HistorialInventario
                .AsNoTracking()
                .AsQueryable();


            // =========================================================
            // 2. FILTROS PRINCIPALES
            // =========================================================
            // Filtros que actúan directamente sobre el historial.
            // No requieren consultar tablas de catálogo.
            // =========================================================

            // ---------------------------------------------------------
            // Producto
            // ---------------------------------------------------------

            if (filter?.IdProducto.HasValue == true)
            {
                query = query.Where(h =>
                    h.IdProducto == filter.IdProducto.Value);
            }


            // ---------------------------------------------------------
            // Género
            // ---------------------------------------------------------

            if (filter?.IdGenero.HasValue == true)
            {
                Console.WriteLine($"Filtro IdGenero = {filter.IdGenero.Value}");

                query = query.Where(h =>
                    h.IdGenero == filter.IdGenero.Value);
            }


            // =========================================================
            // 3. FILTROS DE CATÁLOGOS
            // =========================================================
            // El historial almacena nombres (Referencia, Tela, etc.).
            // El usuario envía IDs.
            // Se realiza la conversión ID -> Descripción.
            // =========================================================

            // ---------------------------------------------------------
            // Referencia
            // ---------------------------------------------------------

            if (filter?.IdReferencia.HasValue == true)
            {
                var referencia = await _context.Referencias
                    .AsNoTracking()
                    .Where(r => r.ID_Referencias == filter.IdReferencia.Value)
                    .Select(r => r.DescripReferencia)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(referencia))
                {
                    query = query.Where(h =>
                        h.Referencia == referencia);
                }
            }


            // ---------------------------------------------------------
            // Talla
            // ---------------------------------------------------------

            if (filter?.IdTalla.HasValue == true)
            {
                var talla = await _context.Tallas
                    .AsNoTracking()
                    .Where(t => t.ID_Tallas == filter.IdTalla.Value)
                    .Select(t => t.DescripTalla)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(talla))
                {
                    query = query.Where(h =>
                        h.Talla == talla);
                }
            }


            // ---------------------------------------------------------
            // Tela
            // ---------------------------------------------------------

            if (filter?.IdTela.HasValue == true)
            {
                var tela = await _context.Telas
                    .AsNoTracking()
                    .Where(t => t.ID_Telas == filter.IdTela.Value)
                    .Select(t => t.DescripTela)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(tela))
                {
                    query = query.Where(h =>
                        h.Tela == tela);
                }
            }


            // ---------------------------------------------------------
            // Color
            // ---------------------------------------------------------

            if (filter?.IdColor.HasValue == true)
            {
                var color = await _context.Colores
                    .AsNoTracking()
                    .Where(c => c.ID_Color == filter.IdColor.Value)
                    .Select(c => c.Nombre)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(color))
                {
                    query = query.Where(h =>
                        h.Color == color);
                }
            }


            // =========================================================
            // 4. FILTRO DE PERÍODO
            // =========================================================
            // Control centralizado de fechas del Kardex.
            //
            // Reglas:
            //
            // ✔ Sin período seleccionado:
            //      No aplica filtro.
            //
            // ✔ MES:
            //      Consulta un mes específico de un año.
            //
            // ✔ ANIO:
            //      Consulta todos los movimientos del año.
            //
            // ✔ PERSONALIZADO:
            //      Consulta rango exacto Desde - Hasta.
            //
            // Este bloque NO modifica filtros de inventario.
            // Únicamente restringe FechaRegistro.
            // =========================================================


            if (!string.IsNullOrEmpty(filter?.TipoPeriodo))
            {

                // =====================================================
                // FILTRO POR MES
                // =====================================================

                if (filter.TipoPeriodo == "MES")
                {

                    if (filter.Mes.HasValue && filter.Anio.HasValue)
                    {

                        var fechaInicioMes = new DateTime(
                            filter.Anio.Value,
                            filter.Mes.Value,
                            1);


                        var fechaFinMes = fechaInicioMes
                            .AddMonths(1)
                            .AddTicks(-1);


                        query = query.Where(h =>
                            h.FechaRegistro >= fechaInicioMes &&
                            h.FechaRegistro <= fechaFinMes);

                    }

                }


                // =====================================================
                // FILTRO POR AÑO
                // =====================================================

                else if (filter.TipoPeriodo == "ANIO")
                {

                    if (filter.Anio.HasValue)
                    {

                        var fechaInicioAnio = new DateTime(
                            filter.Anio.Value,
                            1,
                            1);


                        var fechaFinAnio = fechaInicioAnio
                            .AddYears(1)
                            .AddTicks(-1);


                        query = query.Where(h =>
                            h.FechaRegistro >= fechaInicioAnio &&
                            h.FechaRegistro <= fechaFinAnio);

                    }

                }


                // =====================================================
                // FILTRO PERSONALIZADO
                // =====================================================

                else if (filter.TipoPeriodo == "PERSONALIZADO")
                {

                    if (filter.Desde.HasValue)
                    {

                        var fechaDesde = filter.Desde.Value.Date;


                        query = query.Where(h =>
                            h.FechaRegistro >= fechaDesde);

                    }


                    if (filter.Hasta.HasValue)
                    {

                        var fechaHasta = filter.Hasta.Value.Date
                            .AddDays(1)
                            .AddTicks(-1);


                        query = query.Where(h =>
                            h.FechaRegistro <= fechaHasta);

                    }

                }

            }

            // =========================================================
            // 5. EJECUCIÓN DE LA CONSULTA
            // =========================================================

            Console.WriteLine(query.ToQueryString());

            var lista = await query
                .OrderBy(h => h.FechaRegistro)
                .ThenBy(h => h.Id)
                .ToListAsync();


            // =========================================================
            // 6. CÁLCULO DEL STOCK FINAL
            // =========================================================

            int stockFinal = lista
                .OrderByDescending(x => x.FechaRegistro)
                .Select(x => x.StockActual)
                .FirstOrDefault();


            // =========================================================
            // 7. TRANSFORMACIÓN A VIEWMODEL
            // =========================================================

            var kardex = new List<KardexViewModel>();

            foreach (var h in lista)
            {
                var tipo = KardexTipoMovimientoMapper.GetTipo(
                    h.TipoMovimiento ?? string.Empty);

                bool esEntrada = tipo == TipoMovimientoKardex.Entrada;
                bool esSalida = tipo == TipoMovimientoKardex.Salida;

                int cantidad = Math.Abs(h.Cantidad);

                kardex.Add(new KardexViewModel
                {
                    Fecha = h.FechaRegistro,

                    TipoMovimiento = ObtenerDescripcionMovimiento(h.TipoMovimiento),

                    NombreProducto = h.NombreProducto ?? string.Empty,

                    Referencia = h.Referencia ?? string.Empty,

                    Color = h.Color ?? string.Empty,

                    Tela = h.Tela ?? string.Empty,

                    Talla = h.Talla ?? string.Empty,

                    DocumentoReferencia = h.DocumentoReferencia ?? string.Empty,

                    UsuarioNombre = h.UsuarioNombre ?? string.Empty,

                    Cliente = h.Cliente ?? string.Empty,

                    Observaciones = h.Observaciones ?? string.Empty,

                    EntradaStock = esEntrada ? cantidad : 0,

                    SalidaStock = esSalida ? cantidad : 0,

                    StockAnterior = h.StockAnterior,

                    StockActual = h.StockActual
                });
            }

            // =========================================================
            // 📊 GRÁFICA EVOLUCIÓN MENSUAL DEL INVENTARIO
            // =========================================================

            var grafica = kardex
                .GroupBy(x => new DateTime(
                    x.Fecha.Year,
                    x.Fecha.Month,
                    1))
                .Select(g => new KardexGraficaViewModel
                {
                    Fecha = g.Key,

                    // Producción / compras
                    EntradaStock = g.Sum(x => x.EntradaStock),

                    // Ventas / despachos
                    SalidaStock = g.Sum(x => x.SalidaStock),

                    // 🔥 INVENTARIO REAL AL CIERRE DEL MES
                    // Último movimiento registrado cronológicamente
                    StockActual = g
                    .OrderBy(x => x.Fecha)
                    .Last()
                    .StockActual
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            // =========================================================
            // 📈 RESUMEN MENSUAL PARA TARJETAS UX
            // =========================================================
            var resumenMensual = kardex
    .GroupBy(x => new
    {
        x.Fecha.Year,
        x.Fecha.Month
    })
    .Select(g => new KardexResumenMensualViewModel
    {
        FechaPeriodo = new DateTime(
            g.Key.Year,
            g.Key.Month,
            1),

        Entradas = g.Sum(x => x.EntradaStock),

        Salidas = g.Sum(x => x.SalidaStock),

        StockFinal = g
            .OrderBy(x => x.Fecha)
            .Last()
            .StockActual
    })
    .OrderBy(x => x.FechaPeriodo)
    .ToList();

            // =========================================================
            // ✅ RETURN FINAL
            // =========================================================
            return new KardexResultViewModel
            {
                Movimientos = kardex,

                Grafica = grafica,

                ResumenMensual = resumenMensual,

                TotalEntradas = kardex.Sum(x => x.EntradaStock),

                TotalSalidas = kardex.Sum(x => x.SalidaStock),

                StockFinal = stockFinal
            };
        }

        public async Task<List<StockMinimoViewModel>> ObtenerStockMinimoAsync()
        {
            var movimientos = await _context.HistorialInventario
                .AsNoTracking()
                .ToListAsync();

            var stock = movimientos
                .GroupBy(x => new
                {
                    x.IdProducto,
                    x.Referencia,
                    x.Color,
                    x.Tela,
                    x.Talla
                })
                .Select(g =>
                {
                    var entradas = g.Where(x => x.Cantidad > 0).Sum(x => x.Cantidad);
                    var salidas = g.Where(x => x.Cantidad < 0).Sum(x => Math.Abs(x.Cantidad));

                    return new StockMinimoViewModel
                    {
                        IdProducto = g.Key.IdProducto,
                        Referencia = g.Key.Referencia,
                        Color = g.Key.Color,
                        Tela = g.Key.Tela,
                        Talla = g.Key.Talla,

                        StockActual = entradas - salidas
                    };
                })
                .ToList();

            return stock
                .Where(x => x.StockActual <= 5) // 🔥 umbral mínimo ERP
                .OrderBy(x => x.StockActual)
                .ToList();
        }

        // =========================================================
        // 📦 REGISTRAR ENTRADA DE INVENTARIO (PRODUCCIÓN)
        // =========================================================
        public async Task RegistrarEntradaProduccionAsync(
            Producto producto,
            int cantidad,
            int stockAnterior,
            int stockActual,
            int usuarioId,
            string usuarioNombre,
            int? produccionId,
            string documentoReferencia,
            string? observaciones = null)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            if (cantidad <= 0)
                throw new ArgumentException(
                    "La cantidad debe ser mayor que cero.",
                    nameof(cantidad));

            var movimiento = CrearMovimientoBase(
                tipoMovimiento: "PRODUCCION",
                documentoReferencia: documentoReferencia,
                producto: producto,
                cantidad: cantidad,
                stockAnterior: stockAnterior,
                stockActual: stockActual,
                usuarioId: usuarioId,
                usuarioNombre: usuarioNombre,
                ventaId: null,
                despachoId: null,
                cliente: null,
                observaciones: observaciones
                    ?? $"Entrada por Producción #{produccionId}"
            );

            await RegistrarMovimientoAsync(movimiento);
        }

        // =========================================================
        // 📝 CONVIERTE LOS CÓDIGOS INTERNOS EN DESCRIPCIONES
        // AMIGABLES PARA REPORTES
        // =========================================================
        private static string ObtenerDescripcionMovimiento(string? tipoMovimiento)
        {
            if (string.IsNullOrWhiteSpace(tipoMovimiento))
                return "No definido";

            return tipoMovimiento.Trim().ToUpperInvariant() switch
            {
                "PRODUCCION" => "Producción",

                "VENTA_DESPACHO" => "Despacho de Venta",

                "AJUSTE_POSITIVO" => "Ajuste Positivo de Inventario",

                "AJUSTE_NEGATIVO" => "Ajuste Negativo de Inventario",

                "INGRESO_MANUAL" => "Ingreso Manual",

                "SALIDA_MANUAL" => "Salida Manual",

                "DEVOLUCION_CLIENTE" => "Devolución de Cliente",

                "DEVOLUCION_PRODUCCION" => "Reingreso desde Producción",

                _ => tipoMovimiento
            };
        }

    }
}