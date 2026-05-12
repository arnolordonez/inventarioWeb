namespace InventarioWEB.DTO
{
    public class DetallePedidoProduccionDTO
    {
        public int ID_DetallePedido { get; set; }
        public int ID_Producto { get; set; }
        public int CantidadPedida { get; set; }
        public int CantidadProducida { get; set; }
        public int Pendiente { get; set; }
    }
}
