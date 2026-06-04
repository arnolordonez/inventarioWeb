using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    public class ProduccionCrearViewModel
    {
        // =========================================================
        // CABECERA PRODUCCIÓN
        // =========================================================

        [Required(ErrorMessage = "La fecha de producción es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaProduccion { get; set; } = DateTime.Today;

        [StringLength(255, ErrorMessage = "La observación no puede superar 255 caracteres.")]
        public string? Observaciones { get; set; }

        // =========================================================
        // DETALLES
        // =========================================================

        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto.")]
        public List<DetalleProduccionVM> Detalles { get; set; } = new();
    }

    public class DetalleProduccionVM
    {
        // =========================================================
        // PRODUCTO
        // =========================================================

        [Required]
        public int ID_Producto { get; set; }

        // =========================================================
        // CANTIDAD PRODUCIDA
        // =========================================================

        [Required(ErrorMessage = "La cantidad producida es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Cantidad producida inválida.")]
        public int CantidadProducida { get; set; }

        // =========================================================
        // COSTO
        // =========================================================

        [Required(ErrorMessage = "El costo unitario es obligatorio.")]
        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal CostoUnitario { get; set; }

        // =========================================================
        // CAMPOS COMPLEMENTARIOS
        // =========================================================

        public decimal PrecioVentaUnitario { get; set; }

        public decimal IVA { get; set; }

        // =========================================================
        // TRAZABILIDAD PRODUCCIÓN
        // =========================================================

        public int? ID_DetallePedido { get; set; }

        [StringLength(50)]
        public string EstadoProduccion { get; set; } = "PENDIENTE";

        public DateTime? FechaInicioProduccion { get; set; }

        public DateTime? FechaFinProduccion { get; set; }

        [StringLength(255)]
        public string? ObservacionProduccion { get; set; }
    }
}