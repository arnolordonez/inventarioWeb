using System;
using System.Collections.Generic;
using System.Linq;
namespace InventarioWEB.ViewModels
{
    // ======================================================
    // 🔹 DETALLE DE UNA VENTA EN PROCESO DE REGISTRO
    // ======================================================
    // Responsabilidad:
    // • Representa cada línea de detalle mientras el usuario
    //   está creando una nueva venta.
    // • Es utilizado exclusivamente por VentaVM.
    // • No debe utilizarse para consultas, reportes ni
    //   visualización del historial de ventas.
    // ======================================================
    public class DetalleVentaVM
    {
        public int ID_Producto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioBase { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TotalIVA { get; set; }

        public string EstadoPedido { get; set; } = string.Empty;

        public string EstadoPago { get; set; } = string.Empty;

        public string TipoVenta { get; set; } = string.Empty;

        public int? ID_Despacho { get; set; }

        public string EstadoDespacho { get; set; } = string.Empty;
    }
}
