using System;
using System.Threading.Tasks;
using InventarioWEB.Data;
using InventarioWEB.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioWEB.Services
{
    public class HistorialInventarioService
    {
        private readonly MovimientoVentasDbContext _context;

        public HistorialInventarioService(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 🔥 Registro genérico de movimientos (APPEND-ONLY)
        /// </summary>
        public async Task RegistrarMovimientoAsync(HistorialInventario movimiento)
        {
            if (movimiento == null)
                throw new ArgumentNullException(nameof(movimiento));

            if (string.IsNullOrWhiteSpace(movimiento.TipoMovimiento))
                throw new ArgumentException("TipoMovimiento es obligatorio");
           
            if (string.IsNullOrWhiteSpace(movimiento.SkuArticulo))
                throw new Exception("SkuArticulo es obligatorio");

            if (movimiento.UsuarioId <= 0)
                throw new Exception("UsuarioId inválido");

            if (string.IsNullOrWhiteSpace(movimiento.UsuarioNombre))
                throw new Exception("UsuarioNombre es obligatorio");

            if (string.IsNullOrWhiteSpace(movimiento.DocumentoReferencia))
                movimiento.DocumentoReferencia = "SIN-DOC";

            if (movimiento.Cantidad == 0)
                throw new ArgumentException("Cantidad no puede ser 0");

            movimiento.FechaRegistro = DateTime.UtcNow;

            // 🔥 CORRECTO
            await _context.HistorialInventario.AddAsync(movimiento);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// 🔥 REGISTRO DE DESPACHO (SALIDA REAL DE STOCK)
        /// </summary>
        public async Task RegistrarDespachoAsync(
            string sku,
            string referencia,
            string color,
            string tela,
            string talla,
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
            var movimiento = new HistorialInventario
            {
                TipoMovimiento = "VENTA_DESPACHO",
                DocumentoReferencia = documentoReferencia,

                SkuArticulo = sku,
                Referencia = referencia,
                Color = color,
                Tela = tela,
                Talla = talla,

                Cantidad = -cantidad,

                StockAnterior = stockAnterior,
                StockActual = stockActual,

                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,

                VentaId = ventaId,
                DespachoId = despachoId,

                Cliente = cliente,

                Observaciones = $"Despacho asociado a venta {ventaId}"
            };

            await RegistrarMovimientoAsync(movimiento);
        }

        /// <summary>
        /// 🧾 REGISTRO DE VENTA (NO AFECTA STOCK)
        /// Solo registra la intención de venta. 
        /// El stock se descuenta en el despacho.
        /// </summary>
        public async Task RegistrarVentaAsync(
            string sku,
            string referencia,
            string color,
            string tela,
            string talla,
            int cantidad,
            int stockActual,
            int usuarioId,
            string usuarioNombre,
            int ventaId,
            string cliente,
            string documentoReferencia)
        {
            var movimiento = CrearMovimientoBase(
                tipoMovimiento: "VENTA",
                documentoReferencia: documentoReferencia,
                sku: sku,
                referencia: referencia,
                color: color,
                tela: tela,
                talla: talla,
                cantidad: -cantidad, // 🔥 salida lógica (no afecta stock aún)
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

        /// <summary>
        /// 📦 REGISTRO DE ENTRADA DE INVENTARIO
        /// Aplica para producción, compras o ajustes positivos.
        /// </summary>
        public async Task RegistrarEntradaAsync(
            string sku,
            string referencia,
            string color,
            string tela,
            string talla,
            int cantidad,
            int stockAnterior,
            int stockActual,
            int usuarioId,
            string usuarioNombre,
            string tipoEntrada,
            string documentoReferencia)
        {
            var movimiento = CrearMovimientoBase(
                tipoMovimiento: tipoEntrada, // 🔥 Ej: PRODUCCION, COMPRA, AJUSTE
                documentoReferencia: documentoReferencia,
                sku: sku,
                referencia: referencia,
                color: color,
                tela: tela,
                talla: talla,
                cantidad: cantidad, // 🔥 entrada suma stock
                stockAnterior: stockAnterior,
                stockActual: stockActual,
                usuarioId: usuarioId,
                usuarioNombre: usuarioNombre,
                observaciones: $"Entrada de inventario tipo {tipoEntrada}"
            );

            await RegistrarMovimientoAsync(movimiento);
        }
        /// <summary>
        /// 🧠 Construye un objeto HistorialInventario de forma estandarizada
        /// Evita duplicación de código en ventas, despachos y entradas
        /// </summary>
        private HistorialInventario CrearMovimientoBase(
            string tipoMovimiento,
            string documentoReferencia,
            string sku,
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
            return new HistorialInventario
            {
                // 🔥 Tipo y documento
                TipoMovimiento = tipoMovimiento,
                DocumentoReferencia = documentoReferencia,

                // 🔥 Identificación del producto (SIN ProductoId)
                SkuArticulo = sku,
                Referencia = referencia,
                Color = color,
                Tela = tela,
                Talla = talla,

                // 🔥 Movimiento
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockActual = stockActual,

                // 🔥 Usuario
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,

                // 🔥 Relación opcional
                VentaId = ventaId,
                DespachoId = despachoId,
                Cliente = cliente,
               // Cliente = cliente ?? "N/A",
                // 🔥 Observaciones
                Observaciones = observaciones,

                // ⚠️ Fecha NO se asigna aquí → la controla RegistrarMovimientoAsync
            };
        }
        /// <summary>
        /// 📊 Obtiene el Kardex de un producto por SKU + atributos
        /// </summary>
        public async Task<List<HistorialInventario>> ObtenerKardexAsync(
            string sku,
            string referencia,
            string color,
            string talla,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            var query = _context.HistorialInventario
                .Where(h =>
                    h.SkuArticulo == sku &&
                    h.Referencia == referencia &&
                    h.Color == color &&
                    h.Talla == talla);

            if (desde.HasValue)
                query = query.Where(h => h.FechaRegistro >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(h => h.FechaRegistro <= hasta.Value);

            return await query
                .OrderBy(h => h.FechaRegistro)
                .ToListAsync();
        }
    }
}