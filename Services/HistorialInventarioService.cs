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

            movimiento.FechaRegistro = DateTime.UtcNow;

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
                    Cliente = h.Cliente,

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
        /*
        // =========================================================
        // 🧠 FACTORY CENTRAL DE MOVIMIENTOS (ÚNICA FUENTE DE VERDAD)
        // =========================================================
        private HistorialInventario CrearMovimientoBase(
            string tipoMovimiento,
            string documentoReferencia,
            int idProducto,
            string referencia,
            string color,
            string tela,
            string talla,
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
            if (idProducto <= 0)
                throw new Exception("IdProducto inválido");

            return new HistorialInventario
            {
                TipoMovimiento = tipoMovimiento,
                DocumentoReferencia = documentoReferencia,

                IdProducto = idProducto,
                Referencia = referencia,
                Color = color,
                Tela = tela,
                Talla = talla,

                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockActual = stockActual,

                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,

                VentaId = ventaId,
                DespachoId = despachoId,
                Cliente = cliente,

                Observaciones = observaciones
            };
        }
        */

        /*
        /// =========================================================
        /// 📊 KARDEX SIMPLE POR PRODUCTO (FUENTE REAL DEL SISTEMA)
        /// =========================================================
        public async Task<List<HistorialInventario>> ObtenerKardexAsync(
            int idProducto,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            var query = _context.HistorialInventario
                .AsNoTracking()
                .Where(h => h.IdProducto == idProducto);

            if (desde.HasValue)
                query = query.Where(h => h.FechaRegistro >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(h => h.FechaRegistro <= hasta.Value);

            return await query
                .OrderBy(h => h.FechaRegistro)
                .ThenBy(h => h.Id)
                .ToListAsync();
        }
        */

        /// =========================================================
        /// 📊 KARDEX DE INVENTARIO (MOTOR CENTRAL DEL SISTEMA)
        /// =========================================================
        /// 🔹 Consulta, transforma y calcula el Kardex completo por filtros.
        /// 🔹 Genera movimientos con saldo acumulado.
        /// 🔹 Provee datos para tabla y gráfica.
        /// 🔹 Única fuente de verdad del módulo de inventario.
        /// =========================================================
        public async Task<KardexResultViewModel> ObtenerKardexCompletoAsync(KardexFilterDto filter)
        {
            var query = _context.HistorialInventario
                .AsNoTracking()
                .AsQueryable();

            // 🔵 FILTRO PRINCIPAL
            if (filter?.IdProducto.HasValue == true)
                query = query.Where(h => h.IdProducto == filter.IdProducto.Value);

            // 🔍 FILTROS AUXILIARES
            if (!string.IsNullOrWhiteSpace(filter?.Referencia))
            {
                query = query.Where(h => h.Referencia.Contains(filter.Referencia));
            }
            if (!string.IsNullOrWhiteSpace(filter?.Color))
                query = query.Where(h => h.Color == filter.Color);

            if (!string.IsNullOrWhiteSpace(filter?.Tela))
                query = query.Where(h => h.Tela == filter.Tela);

            if (!string.IsNullOrWhiteSpace(filter?.Talla))
                query = query.Where(h => h.Talla == filter.Talla);

            if (filter?.Desde.HasValue == true)
                query = query.Where(h => h.FechaRegistro >= filter.Desde.Value);

            if (filter?.Hasta.HasValue == true)
                query = query.Where(h => h.FechaRegistro <= filter.Hasta.Value);

            var lista = await query
                .OrderBy(h => h.FechaRegistro)
                .ThenBy(h => h.Id)
                .ToListAsync();

            // =========================================================
            // 🔄 TRANSFORMACIÓN KARDEX REAL (ROBUSTO ERP)
            // =========================================================
            int saldo = 0;
            var kardex = new List<KardexViewModel>();

            foreach (var h in lista)
            {
                var tipo = KardexTipoMovimientoMapper.GetTipo(h.TipoMovimiento ?? string.Empty);

                // ⚠️ Auditoría de datos sucios (ANTES de cualquier lógica)
                if (string.IsNullOrWhiteSpace(h.TipoMovimiento))
                {
                    _logger?.LogWarning("Movimiento sin TipoMovimiento. Id: {Id}", h.Id);
                }

                bool esEntrada = tipo == TipoMovimientoKardex.Entrada;
                bool esSalida = tipo == TipoMovimientoKardex.Salida;

                int cantidad = Math.Abs(h.Cantidad);

                int entrada = esEntrada ? cantidad : 0;
                int salida = esSalida ? cantidad : 0;

                saldo += entrada - salida;

                kardex.Add(new KardexViewModel
                {
                    Fecha = h.FechaRegistro,
                    TipoMovimiento = h.TipoMovimiento,
                    Referencia = h.Referencia,
                    Color = h.Color,
                    Tela = h.Tela,
                    Talla = h.Talla,
                    DocumentoReferencia = h.DocumentoReferencia,
                    UsuarioNombre = h.UsuarioNombre,
                    Entrada = entrada,
                    Salida = salida,
                    Saldo = saldo
                });
            }

            // =========================================================
            // 📊 GRÁFICA (FUERA DEL LOOP - CORRECTO)
            // =========================================================

            var grafica = kardex
                .GroupBy(x => x.Fecha.Date)
                .Select(g => new KardexGraficaViewModel
                {
                    Fecha = g.Key,
                    Entrada = g.Sum(x => x.Entrada),
                    Salida = g.Sum(x => x.Salida)
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            // =========================================================
            // ✅ RETURN FINAL
            // =========================================================

            return new KardexResultViewModel
            {
                Movimientos = kardex,
                Grafica = grafica,
                TotalEntradas = kardex.Sum(x => x.Entrada),
                TotalSalidas = kardex.Sum(x => x.Salida)
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

    }
}