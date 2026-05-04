namespace InventarioWEB.Models
{
    public class EnvioWhatsApp
    {
        public int Id { get; set; }

        public int IdPedido { get; set; }
        public Pedido IdPedidoNavigation { get; set; }

        public int IdCliente { get; set; }
        public Cliente IdClienteNavigation { get; set; }

        public string Telefono { get; set; }

        public string UrlPdf { get; set; }

        public DateTime FechaEnvio { get; set; }

        public string Estado { get; set; }
    }
}
