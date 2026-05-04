namespace InventarioWEB.ViewModels
{
    public class DashboardViewModel
    {
        public decimal VentasHoy { get; set; } = 0;
        public int TotalDespachos { get; set; } = 0;

        public int StockTotal { get; set; } = 0;
        public int StockBajo { get; set; } = 0;

        public int ProduccionActiva { get; set; } = 0;
    }
}