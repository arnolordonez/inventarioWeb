namespace InventarioWEB.ViewModels
{
    public class PendienteProduccionVM
    {
        // =====================================================
        // PEDIDO
        // =====================================================

        public int ID_DetallePedido { get; set; }

        public int ID_Pedido { get; set; }

        public DateTime FechaPedido { get; set; }

        // =====================================================
        // PRODUCTO
        // =====================================================

        public int ID_Producto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string Referencia { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        // =====================================================
        // CLIENTE
        // =====================================================

        public string Cliente { get; set; } = string.Empty;

        // =====================================================
        // ESTADOS
        // =====================================================

        // NO DESPACHADO / DESPACHADO
        public string Estado { get; set; } = string.Empty;

        // PENDIENTE / ABONADO / PAGADO
        public string EstadoPago { get; set; } = string.Empty;

        // CONTADO / CREDITO
        public string TipoVenta { get; set; } = string.Empty;

        // =====================================================
        // VALORES
        // =====================================================

        public decimal TotalVenta { get; set; }

        public decimal Saldo { get; set; }

        // =====================================================
        // PRODUCCIÓN
        // =====================================================

        public int CantidadPedida { get; set; }

        public int CantidadProducida { get; set; }

        public int CantidadPendiente { get; set; }

        // PENDIENTE / EN PRODUCCIÓN / COMPLETADO
        public string EstadoProduccion { get; set; } = "PENDIENTE";
    }
}