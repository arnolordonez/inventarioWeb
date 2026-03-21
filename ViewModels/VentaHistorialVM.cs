using System;

namespace InventarioWEB.ViewModels
{
    public class VentaHistorialVM
    {
        public int ID_Pedido { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public decimal TotalVenta { get; set; }

        public decimal TotalAbonado { get; set; }

        public decimal Saldo { get; set; }

        // 🔥 CALCULADO
        public string Estado => Saldo == 0 ? "Pagado" : "Pendiente";
    }
}