namespace InventarioWEB.ViewModels
{
    public class OrdenProduccionVM
    {
        public int IdPedido { get; set; }
        public string Fecha { get; set; }
        public string Estado { get; set; }
        public string TipoVenta { get; set; }

        public List<DetalleOrdenVM> Detalles { get; set; }
    }

    public class DetalleOrdenVM
    {
        public int ID_Producto { get; set; } // 🔥 SE MANTIENE
        public string Producto { get; set; }
        public string Color { get; set; }
        public string Talla { get; set; }
        public int Cantidad { get; set; }
    }
}