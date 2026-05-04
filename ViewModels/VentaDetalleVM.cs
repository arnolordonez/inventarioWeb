using System;
using System.Collections.Generic;

namespace InventarioWEB.ViewModels
{
    // ======================================================
    // 🔹 DETALLE COMPLETO DE UNA VENTA (VISTA DETALLE)
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

        public List<DetalleProductoVM> Productos { get; set; } = new();

        public List<AbonoDetalleVM> Abonos { get; set; } = new();
    }

    // ======================================================
    // 🔹 PRODUCTOS (MOSTRAR EN DETALLE)
    // ======================================================
    public class DetalleProductoVM
    {
        // 🔥 ESTE FALTABA
        public int ID_Producto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int Cantidad { get; set; }

       // public int Cantidad_Despachada { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal Subtotal { get; set; }
    }

    // ======================================================
    // 🔹 ABONOS (MOSTRAR EN DETALLE)
    // ======================================================
    public class AbonoDetalleVM
    {
        public DateTime Fecha { get; set; }

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        // 🔥 OPCIONAL PERO RECOMENDADO
        public int ID_MetodoPago { get; set; }
    }

    // ======================================================
    // 🔥 ESTE ES EL QUE USA EL FRONTEND (JS)
    // ======================================================
    public class DetalleVentaVM
    {
        public int ID_Producto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioBase { get; set; }

        public decimal PrecioVenta { get; set; }

        // 🔥 ESTE ES EL QUE NO SABÍAS DÓNDE VA → VA AQUÍ
        public decimal Subtotal => Cantidad * PrecioVenta;
    }
}