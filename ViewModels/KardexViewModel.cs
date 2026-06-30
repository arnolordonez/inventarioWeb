namespace InventarioWEB.ViewModels
{
    public class KardexViewModel
    {
        public DateTime Fecha { get; set; }

        public string? TipoMovimiento { get; set; }

        public string? Referencia { get; set; }
        public string? Color { get; set; }
        public string? Tela { get; set; }
        public string? Talla { get; set; }

        // 🔥 Nombres alineados con la BD
        public string? DocumentoReferencia { get; set; }
        public string? UsuarioNombre { get; set; }

        public int Entrada { get; set; }
        public int Salida { get; set; }
        public int Saldo { get; set; }

        public List<KardexViewModel> Movimientos { get; set; } = new();

        public object Grafica { get; set; } = new { };

        public int TotalEntradas { get; set; }

        public int TotalSalidas { get; set; }
    }
}