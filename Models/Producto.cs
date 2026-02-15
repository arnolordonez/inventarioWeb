using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("productos")]
    public class Producto
    {
        // ============================
        // CLAVE PRIMARIA
        // ============================
        [Key]
        [Column("ID_Producto")]
        public int ID_Producto { get; set; }

        // ============================
        // IDENTIDAD DEL PRODUCTO
        // ============================
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // SNAPSHOT HISTÓRICO (NO FK)
        // ⚠️ RENOMBRADO para evitar colisión con entidad Color
        [Column("Color")]
        [StringLength(150)]
        public string? ColorSnapshot { get; set; }

        // ============================
        // RELACIONES (FK)
        // ============================
        [Required]
        public int ID_Referencias { get; set; }

        [Required]
        public int ID_Tallas { get; set; }

        [Required]
        public int ID_Telas { get; set; }

        [Required]
        public int ID_Color { get; set; }

        // ============================
        // INVENTARIO Y PRECIOS
        // ============================
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCosto { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVTA { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal IVA_Porcentaje { get; set; }

        public int Stock { get; set; }

        public bool Activo { get; set; }

        // ============================
        // NAVEGACIÓN
        // ============================
        [ForeignKey(nameof(ID_Tallas))]
        public Talla? Talla { get; set; }

        [ForeignKey(nameof(ID_Referencias))]
        public Referencia? Referencia { get; set; }

        [ForeignKey(nameof(ID_Telas))]
        public Tela? Tela { get; set; }

        [ForeignKey(nameof(ID_Color))]
        public Color? ColorNav { get; set; }
    }
}
