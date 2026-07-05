using System;
using System.Collections.Generic;
using System.Linq;

namespace InventarioWEB.ViewModels
{
    // ======================================================
    // 🔹 DETALLE COMPLETO DE UNA VENTA
    // ======================================================
    public class VentaDetalleVM
    {
        public int ID_Pedido { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public decimal TotalVenta { get; set; }

        public decimal TotalAbonado { get; set; }

        public decimal Saldo => TotalVenta - TotalAbonado;
        public string Estado => Saldo == 0 ? "Pagado" : "Pendiente";

        public string TipoVenta { get; set; } = string.Empty;

        public string EstadoPedido { get; set; } = string.Empty;

        public string EstadoPago { get; set; } = string.Empty;

        public string EstadoDespacho { get; set; } = string.Empty;

        public int? ID_Despacho { get; set; }
              
        public int TotalProductos => Productos.Count;

        public int TotalUnidades => Productos.Sum(x => x.Cantidad);

        public List<DetalleProductoVM> Productos { get; set; } = new();
        public List<AbonoDetalleVM> Abonos { get; set; } = new(); // ✔ AQUÍ
    }

    // ======================================================
    // 🔹 PRODUCTOS DE LA VENTA
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

    // ======================================================
    // 🔹 VIEWMODEL UTILIZADO AL CREAR UNA VENTA
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