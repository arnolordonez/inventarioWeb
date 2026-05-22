using InventarioWEB.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    // =========================================================
    // DETALLE DESPACHO ERP
    // =========================================================

    [Table("detalle_despacho")]

    // =========================================================
    // UNICIDAD ERP
    // Evita duplicar el mismo detalle pedido
    // dentro del mismo despacho
    // =========================================================

    [Index(
        nameof(ID_Despacho),
        nameof(ID_Detalle),
        IsUnique = true)]

    public class DetalleDespacho
    {
        // =====================================================
        // PK
        // =====================================================

        [Key]
        [Column("ID_DetalleDespacho")]
        public int ID_DetalleDespacho { get; set; }

        // =====================================================
        // DESPACHO
        // =====================================================

        [Required]
        public int ID_Despacho { get; set; }

        // =====================================================
        // PEDIDO DETALLE ERP
        // =====================================================

        [Required]
        [Column("ID_Detalle")]
        public int ID_Detalle { get; set; }

        // =====================================================
        // PRODUCTO
        // =====================================================

        [Required]
        public int ID_Producto { get; set; }

        // =====================================================
        // PRODUCCIÓN RELACIONADA
        // TRAZABILIDAD ERP
        // =====================================================

        public int? ID_Detalle_Produccion { get; set; }

        // =====================================================
        // CANTIDAD
        // =====================================================

        [Required]
        public int Cantidad_Despachada { get; set; }


        /*
        // =====================================================
        // AUDITORÍA ERP
        // =====================================================

        public DateTime FechaRegistro { get; set; }
            = DateTime.Now;

        [StringLength(100)]
        public string UsuarioRegistro { get; set; }
            = "Sistema";
        */

        // =====================================================
        // NAVEGACIÓN
        // =====================================================

        [ForeignKey(nameof(ID_Despacho))]
        public virtual Despacho Despacho { get; set; } = null!;

        [ForeignKey(nameof(ID_Producto))]
        public virtual Producto Producto { get; set; } = null!;

        [ForeignKey(nameof(ID_Detalle))]
        public virtual DetallePedido DetallePedido { get; set; } = null!;

        // =====================================================
        // PRODUCCIÓN ERP
        // =====================================================

        [ForeignKey(nameof(ID_Detalle_Produccion))]
        public virtual DetalleProduccion? DetalleProduccion { get; set; }
    }
}