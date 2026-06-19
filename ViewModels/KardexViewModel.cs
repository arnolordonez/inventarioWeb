namespace InventarioWEB.ViewModels
{
    public class KardexViewModel
    {
        public DateTime Fecha { get; set; }

        public string? TipoMovimiento { get; set; }
        public string? Sku { get; set; }
        public string? Referencia { get; set; }
        public string? Color { get; set; }
        public string? Tela { get; set; }           // ✅ NUEVO
        public string? Talla { get; set; }

        public string? Documento { get; set; }      // ✅ NUEVO
        public string? Usuario { get; set; }        // ✅ NUEVO

        public int Entrada { get; set; }
        public int Salida { get; set; }
        public int Saldo { get; set; }
    }
}