using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("detalle_produccion")]
    public class DetalleProduccion
    {
        [Key]
        [Column("ID_Detalle_Produccion")]
        public int ID_DetalleProduccion { get; set; }

        [Required]
        [Column("ID_Produccion")]
        public int ID_Produccion { get; set; }

        [Required]
        [Column("ID_Producto")]
        public int ID_Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVentaUnitario { get; set; }

        // porcentaje de IVA (ej: 19)
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; }

        // valores calculados en el servicio
        [Column(TypeName = "decimal(12,2)")]
        public decimal SubtotalCosto { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal SubtotalVenta { get; set; }

        // relaciones
        [ForeignKey("ID_Produccion")]
        public virtual Produccion? Produccion { get; set; }

        [ForeignKey("ID_Producto")]
        public virtual Producto? Producto { get; set; }
    }
}
