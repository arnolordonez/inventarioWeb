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

        [StringLength(255,
            ErrorMessage = "La observación no puede superar 255 caracteres.")]
        public string? Observaciones { get; set; }

        // =========================================================
        // PEDIDO RELACIONADO
        // =========================================================

        public int? ID_Pedido { get; set; }

        // En tu sistema corresponde al documento/NIT
        public int? ID_Cliente { get; set; }

        public string Cliente { get; set; } = string.Empty;

        // ABONADO / PAGADO / PENDIENTE
        public string EstadoPago { get; set; } = string.Empty;

        // DESPACHADO / NO DESPACHADO
        public string Estado { get; set; } = string.Empty;

        // CONTADO / CREDITO
        public string TipoVenta { get; set; } = string.Empty;

        public decimal TotalPedido { get; set; }

        public decimal SaldoPendiente { get; set; }

        // =========================================================
        // DETALLES
        // =========================================================

        [MinLength(1,
            ErrorMessage = "Debe agregar al menos un producto.")]
        public List<DetalleProduccionVM> Detalles { get; set; }
            = new List<DetalleProduccionVM>();
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

        [Required(
            ErrorMessage = "La cantidad producida es obligatoria.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Cantidad producida inválida.")]
        public int CantidadProducida { get; set; }

        // =========================================================
        // CONTROL PRODUCCIÓN (NUEVO)
        // =========================================================

        // Cantidad pendiente por producir (solo lectura en UI)
        public int CantidadPendiente { get; set; }

        // =========================================================
        // COSTO
        // =========================================================

        [Required(
            ErrorMessage = "El costo unitario es obligatorio.")]
        [Range(typeof(decimal), "0.01", "999999999",
            ErrorMessage = "Costo unitario inválido.")]
        public decimal CostoUnitario { get; set; }

        // =========================================================
        // CAMPOS COMPLEMENTARIOS
        // =========================================================

        public decimal PrecioVentaUnitario { get; set; }

        public decimal IVA { get; set; }

        // =========================================================
        // TRAZABILIDAD PRODUCCIÓN
        // =========================================================

        public int ID_DetallePedido { get; set; }

        [StringLength(50)]
        public string EstadoProduccion { get; set; }
            = "PENDIENTE";

        public DateTime? FechaInicioProduccion { get; set; }

        public DateTime? FechaFinProduccion { get; set; }

        [StringLength(255)]
        public string? ObservacionProduccion { get; set; }
    }
}