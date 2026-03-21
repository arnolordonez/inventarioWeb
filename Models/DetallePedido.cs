using InventarioWEB.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("detalle_pedido")]

    // 🔥 EVITA DUPLICADOS DEL MISMO PRODUCTO EN UN PEDIDO
    [Index(nameof(ID_Pedido), nameof(ID_Producto), IsUnique = true)]

    public class DetallePedido
    {
        // ============================
        // 🔑 CLAVE PRIMARIA
        // ============================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID_Detalle")]
        public int ID_Detalle { get; set; }

        // ============================
        // 🔗 RELACIONES (FK)
        // ============================
        [Required]
        [Column("ID_Pedido")]
        public int ID_Pedido { get; set; }

        [Required]
        [Column("ID_Producto")]
        public int ID_Producto { get; set; }

        // ============================
        // 📦 DATOS DE VENTA
        // ============================
        [Required]
        [Column("Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("PrecioBase", TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }

        [Required]
        [Column("PrecioVenta", TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [Column("Subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        // ============================
        // 🚚 CONTROL DE DESPACHO
        // ============================
        [Required]
        [Column("Cantidad_Despachada")]
        public int Cantidad_Despachada { get; set; } = 0;

        // ============================
        // 🔗 NAVEGACIÓN
        // ============================
        [ForeignKey(nameof(ID_Pedido))]
        public Pedido Pedido { get; set; } = null!;

        [ForeignKey(nameof(ID_Producto))]
        public Producto Producto { get; set; } = null!;
    }
}