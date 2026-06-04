namespace InventarioWEB.ViewModels
{
    public class ProduccionPedidoItemVM
    {
        // =====================================================
        // PEDIDO
        // =====================================================

        public int ID_Pedido { get; set; }

        public DateTime FechaPedido { get; set; }

        // =====================================================
        // CLIENTE
        // =====================================================

        public int ID_Cliente { get; set; }

        public string Cliente { get; set; } = string.Empty;

        // =====================================================
        // ESTADOS
        // =====================================================

        // NO DESPACHADO / DESPACHADO
        public string Estado { get; set; } = string.Empty;

        // ABONADO / PAGADO 
        public string EstadoPago { get; set; } = string.Empty;

        // CONTADO / CREDITO
        public string TipoVenta { get; set; } = string.Empty;

        // =====================================================
        // VALORES
        // =====================================================

        public decimal TotalVenta { get; set; }

        public decimal SaldoPendiente { get; set; }

        // =====================================================
        // PRODUCCIÓN
        // =====================================================

        public int TotalPedido { get; set; }

        public int TotalProducido { get; set; }

        public int Pendiente { get; set; }

        public decimal PorcentajeProduccion { get; set; }

        // PENDIENTE / EN PRODUCCIÓN / COMPLETADO
        public string EstadoProduccion { get; set; } = string.Empty;

        public DateTime? UltimaProduccion { get; set; }
    }
}