namespace InventarioWEB.ViewModels
{
    public class HistorialVentasResultadoVM
    {
        public List<VentaHistorialVM> Ventas { get; set; } = new();

        public int TotalRegistros { get; set; }

        public int TotalPagadas { get; set; }

        public int TotalAbonadas { get; set; }

        public decimal TotalSaldoPendiente { get; set; }

        public int PaginaActual { get; set; }

        public int TotalPaginas { get; set; }
    }
}
