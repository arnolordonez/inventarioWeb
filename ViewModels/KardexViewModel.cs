namespace InventarioWEB.ViewModels
{
    public class KardexViewModel
    {
        public DateTime Fecha { get; set; }

        public string TipoMovimiento { get; set; } = string.Empty;

        public string Referencia { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Tela { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;

        public string DocumentoReferencia { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;


        // ================================
        // 🔄 MOVIMIENTO (DELTA)
        // ================================
        public int EntradaStock { get; set; }
        public int SalidaStock { get; set; }

        // ================================
        // 📦 ESTADO DEL INVENTARIO
        // ================================
        public int StockAnterior { get; set; }
        public int StockActual { get; set; }

        // ================================
        // 📊 CAMPOS UI (GRÁFICA / UX)
        // ================================
        public object? GraficaData { get; set; }
    }
}