using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("detalle_produccion")]
    public class DetalleProduccion
    {
        // =====================================================
        // PK
        // =====================================================

        [Key]
        [Column("ID_Detalle_Produccion")]
        public int ID_DetalleProduccion { get; set; }

        // =====================================================
        // FK PRODUCCIÓN
        // =====================================================

        [Required]
        [Column("ID_Produccion")]
        public int ID_Produccion { get; set; }

        // =====================================================
        // FK PRODUCTO
        // =====================================================

        [Required]
        [Column("ID_Producto")]
        public int ID_Producto { get; set; }

        // =====================================================
        // CANTIDAD PRODUCIDA
        // =====================================================

        [Required]
        [Column("CantidadProducida")]
        public int CantidadProducida { get; set; }

        // =====================================================
        // COSTOS
        // =====================================================

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVentaUnitario { get; set; }

        // =====================================================
        // IVA
        // =====================================================

        // porcentaje IVA (ej: 19)
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; }

        // =====================================================
        // SUBTOTALES
        // =====================================================

        [Column(TypeName = "decimal(12,2)")]
        public decimal SubtotalCosto { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal SubtotalVenta { get; set; }

        // =====================================================
        // VÍNCULO PEDIDO
        // =====================================================

        [Column("ID_DetallePedido")]
        public int? ID_DetallePedido { get; set; }

        // =====================================================
        // CONTROL OPERACIONAL
        // =====================================================

        [Required]
        [StringLength(50)]
        public string EstadoProduccion { get; set; } = "PENDIENTE";

        public DateTime? FechaInicioProduccion { get; set; }

        public DateTime? FechaFinProduccion { get; set; }

        [StringLength(255)]
        public string? ObservacionProduccion { get; set; }

        // =====================================================
        // RELACIONES
        // =====================================================

        [ForeignKey(nameof(ID_Produccion))]
        public virtual Produccion Produccion { get; set; } = null!;

        [ForeignKey(nameof(ID_Producto))]
        public virtual Producto Producto { get; set; } = null!;

        [ForeignKey(nameof(ID_DetallePedido))]
        public virtual DetallePedido? DetallePedido { get; set; }
    }
}