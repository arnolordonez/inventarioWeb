using System;

namespace InventarioWEB.ViewModels
{
    public class ProductoProduccionDTO
    {
        public int ID_Producto { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;  // ✅ AGREGADO

        public string Referencia { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        public string Tela { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int Stock { get; set; }

        public decimal PrecioCosto { get; set; }

        public decimal PrecioVTA { get; set; }
    }
}