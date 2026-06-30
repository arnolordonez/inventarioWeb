namespace InventarioWEB.ViewModels
{
    public class StockMinimoViewModel
    {
        public int IdProducto { get; set; }

        public string? Referencia { get; set; }
        public string? Color { get; set; }
        public string? Tela { get; set; }
        public string? Talla { get; set; }

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }

        public bool Alerta => StockActual <= StockMinimo;
    }
}