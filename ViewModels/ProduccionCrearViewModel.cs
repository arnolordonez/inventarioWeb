using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    public class ProduccionCrearViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaProduccion { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto.")]
        public List<DetalleProduccionVM> Detalles { get; set; } = new();
    }

    public class DetalleProduccionVM
    {
        [Required]
        public int ID_Producto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Cantidad inválida.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El costo unitario es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Costo inválido.")]
        public decimal CostoUnitario { get; set; }
    }
}