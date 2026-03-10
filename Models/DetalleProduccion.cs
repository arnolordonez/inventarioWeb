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
        public int ID_Produccion { get; set; }

        [Required]
        public int ID_Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVentaUnitario { get; set; }

        // ⚠️ ESTE ES PORCENTAJE (ej: 19)
        [Column(TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; }

        // ⚠️ CALCULADOS EN SERVICE
        [Column(TypeName = "decimal(12,2)")]
        public decimal SubtotalCosto { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal SubtotalVenta { get; set; }

        [ForeignKey(nameof(ID_Produccion))]
        public Produccion? Produccion { get; set; }

        [ForeignKey(nameof(ID_Producto))]
        public Producto? Producto { get; set; }
    }
}