using System;
using System.Collections.Generic;
using System.Linq;

namespace InventarioWEB.ViewModels
{
    // ======================================================
    // 🔹 PRODUCTOS ASOCIADOS A UNA VENTA CONSULTADA
    // ======================================================
    // Responsabilidad:
    // • Representa cada producto incluido en una venta.
    // • Se utiliza únicamente dentro de VentaDetalleVM.
    // • Alimenta la tabla "Productos Vendidos" del detalle
    //   y de los reportes comerciales.
    // ======================================================
    public class DetalleProductoVM
    {
        public int ID_Producto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal Subtotal { get; set; }
    }
}
