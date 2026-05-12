namespace InventarioWEB.ViewModels
{
    public class OrdenProduccionVM
    {
        public int IdPedido { get; set; }

        public string Fecha { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string TipoVenta { get; set; } = string.Empty;

        public List<DetalleOrdenVM> Detalles { get; set; } = new();
    }

    public class DetalleOrdenVM
    {
        // =====================================================
        // PRODUCTO
        // =====================================================

        public int ID_Producto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        // =====================================================
        // CANTIDAD
        // =====================================================

        public int Cantidad { get; set; }
    }
}