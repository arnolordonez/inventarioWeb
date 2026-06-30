namespace InventarioWEB.ViewModels
{
    public class KardexAnaliticoViewModel
    {
        // 🔥 Identificación del producto
        public int IdProducto { get; set; }

        public string? Referencia { get; set; }
        public string? Genero { get; set; }
        public string? Color { get; set; }
        public string? Tela { get; set; }

        // 📦 Totales del período
        public int TotalEntradas { get; set; }
        public int TotalSalidas { get; set; }

        // 📊 Stock calculado o final
        public int StockActual { get; set; }

        // 🔢 Cantidad de movimientos
        public int MovimientosTotales { get; set; }

        // 🕒 Último movimiento del producto
        public DateTime? UltimoMovimiento { get; set; }

        public string? UltimoTipoMovimiento { get; set; }
    }
}
