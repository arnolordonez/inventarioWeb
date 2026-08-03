namespace InventarioWEB.ViewModels
{
    public class KardexGraficaViewModel
    {
        public DateTime Fecha { get; set; }

        public int EntradaStock { get; set; }

        public int SalidaStock { get; set; }
        public int StockActual { get; set; }
    }
}