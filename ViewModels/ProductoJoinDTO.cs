using InventarioWEB.Models;

namespace InventarioWEB.ViewModels
{
    public class ProductoJoinDTO
    {
        public Producto Producto { get; set; } = null!;
        public Referencia Referencia { get; set; } = null!;
        public Talla Talla { get; set; } = null!;
        public Tela Tela { get; set; } = null!;
        public Color Color { get; set; } = null!;
    }
}