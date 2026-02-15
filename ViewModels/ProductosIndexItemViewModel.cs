namespace InventarioWEB.ViewModels
{
    public class ProductosIndexItemViewModel
    {
        public int ID_Producto { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string Referencia { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;
        public string Tela { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public decimal PrecioVTA { get; set; }
        public decimal IVA_Porcentaje { get; set; }

        public bool Activo { get; set; }
    }
}
