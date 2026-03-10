using InventarioWEB.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("detalle_pedido")]
    [Index(nameof(ID_Pedido), nameof(ID_Producto), IsUnique = true)] // Evita duplicados
    public class DetallePedido
    {
        [Key]
        [Column("ID_Detalle")]
        public int ID_Detalle { get; set; }

        [Required]
        [Column("ID_Pedido")]
        public int ID_Pedido { get; set; }

        [Required]
        [Column("ID_Producto")]
        public int ID_Producto { get; set; }

        [Required]
        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("Precio_Unitario", TypeName = "decimal(10,2)")]
        public decimal Precio_Unitario { get; set; }

        [Required]
        [Column("Subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        // 🔥 CAMPO NECESARIO PARA CONTROL DE DESPACHO
        [Required]
        [Column("Cantidad_Despachada")]
        public int Cantidad_Despachada { get; set; } = 0;

        // ================================
        // RELACIONES
        // ================================
        [ForeignKey("ID_Pedido")]
        public virtual Pedido Pedido { get; set; } = null!;

        [ForeignKey("ID_Producto")]
        public virtual Producto Producto { get; set; } = null!;
    }
}