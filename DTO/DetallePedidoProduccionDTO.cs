namespace InventarioWEB.DTO
{
    public class DetallePedidoProduccionDTO
    {
        // =====================================================
        // IDENTIFICADORES
        // =====================================================

        public int ID_DetallePedido { get; set; }

        public int ID_Producto { get; set; }

        // =====================================================
        // PRODUCTO
        // =====================================================

        public string Producto { get; set; } = string.Empty;

        public string Referencia { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;


        // =====================================================
        // CANTIDADES
        // =====================================================

        // Cantidad solicitada en el pedido
        public int CantidadPedido { get; set; }

        // Cantidad acumulada ya producida
        public int CantidadProducida { get; set; }

        // Cantidad pendiente por producir
        public int Pendiente { get; set; }



        // =====================================================
        // INVENTARIO
        // =====================================================

        public int StockActual { get; set; }

        // =====================================================
        // PRECIOS
        // =====================================================

        public decimal PrecioCosto { get; set; }

        public decimal PrecioVTA { get; set; }

        public decimal IVA_Porcentaje { get; set; }
    }
}