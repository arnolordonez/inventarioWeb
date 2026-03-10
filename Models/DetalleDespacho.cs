using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("detalle_despacho")]

    // 🔥 Evita duplicar el mismo producto en un despacho
    [Index(nameof(ID_Despacho), nameof(ID_Producto), IsUnique = true)]

    // 🔥 Índices para consultas rápidas
    [Index(nameof(ID_Despacho))]
    [Index(nameof(ID_Producto))]

    public class DetalleDespacho
    {
        // =========================================
        // 🔑 CLAVE PRIMARIA
        // =========================================

        [Key]
        [Column("ID_DetalleDespacho")]
        public int ID_DetalleDespacho { get; set; }


        // =========================================
        // 🔗 RELACIONES
        // =========================================

        [Required]
        [Column("ID_Despacho")]
        public int ID_Despacho { get; set; }

        [Required]
        [Column("ID_Producto")]
        public int ID_Producto { get; set; }


        // =========================================
        // 📦 CANTIDAD DESPACHADA
        // =========================================

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        [Column("Cantidad_Despachada")]
        public int Cantidad_Despachada { get; set; }


        // =========================================
        // 🔗 NAVEGACIÓN
        // =========================================

        [ForeignKey(nameof(ID_Despacho))]
        public virtual Despacho Despacho { get; set; } = null!;

        [ForeignKey(nameof(ID_Producto))]
        public virtual Producto Producto { get; set; } = null!;
    }
}