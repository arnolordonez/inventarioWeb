using System;
using System;

namespace InventarioWEB.ViewModels
{
    public class VentaHistorialVM
    {
        // =========================
        // IDENTIFICADOR DE VENTA
        // =========================
        public int ID_Pedido { get; set; }

        // =========================
        // INFORMACIÓN COMERCIAL
        // =========================
        public string Cliente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string TipoVenta { get; set; } = string.Empty;

        // =========================
        // ESTADOS REALES DEL SISTEMA
        // =========================
       
        public string EstadoPedido { get; set; } = string.Empty;

        public string EstadoPago { get; set; } = string.Empty;

        public string EstadoDespacho { get; set; } = string.Empty;

        // =========================
        // VALORES FINANCIEROS
        // =========================
        public decimal Subtotal { get; set; }

        public decimal TotalIVA { get; set; }

        public decimal TotalVenta { get; set; }

        public decimal Saldo { get; set; }

        public decimal TotalAbonado { get; set; }

        // =========================
        // RESUMEN DE PRODUCTOS (DERIVADO)
        // =========================
        public int TotalProductos { get; set; }

        public int TotalUnidades { get; set; }

        // =========================
        // FACTURA (HOY = DESPACHO)
        // =========================
        public int? ID_Despacho { get; set; }

        public int? NumeroFactura => ID_Despacho;

        // =========================
        // ESTADO CALCULADO (MEJORADO)
        // =========================
        public string Estado
        {
            get
            {
                if (EstadoPago == "PAGADO")
                    return "Pagado";

                if (EstadoPago == "ABONADO")
                    return "Parcial";

                return "Pendiente";
            }
        }
    }
}

