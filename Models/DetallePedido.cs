using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("detalle_pedido")]
    public class DetallePedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Detalle { get; set; }

        [Required]
        public int ID_Pedido { get; set; }

        [Required]
        public int ID_Producto { get; set; }  // ✅ corregido

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio_Unitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        // 🔗 Relaciones
        [ForeignKey(nameof(ID_Pedido))]
        public virtual Pedido Pedido { get; set; } = null!;

        [ForeignKey(nameof(ID_Producto))]
        public virtual Producto Producto { get; set; } = null!;
    }
}
