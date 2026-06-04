namespace InventarioWEB.ViewModels
{
    public class PendienteProduccionVM
    {
        public int ID_DetallePedido { get; set; }

        public int ID_Pedido { get; set; }

        public int ID_Producto { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public string Producto { get; set; } = string.Empty;

        public string Referencia { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int CantidadPedida { get; set; }

        public int CantidadProducida { get; set; }

        public int CantidadPendiente { get; set; }

        public string EstadoProduccion { get; set; } = "PENDIENTE";
    }

}

