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
        [Column("Nombre")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // ============================
        // SNAPSHOT HISTÓRICO DEL COLOR
        // ============================
        [Column("Color")]
        [StringLength(150)]
        public string? ColorSnapshot { get; set; }

        // ============================
        // CLAVES FORÁNEAS
        // ============================
        [Required]
        [Column("ID_Genero")]
        public int ID_Genero { get; set; }

        [Required]
        [Column("ID_Referencias")]
        public int ID_Referencias { get; set; }

        [Required]
        [Column("ID_Tallas")]
        public int ID_Tallas { get; set; }

        [Required]
        [Column("ID_Telas")]
        public int ID_Telas { get; set; }

        [Required]
        [Column("ID_Color")]
        public int ID_Color { get; set; }

        // ============================
        // PRECIOS
        // ============================
        [Column("PrecioCosto", TypeName = "decimal(10,2)")]
        public decimal PrecioCosto { get; set; }

        [Column("PrecioVTA", TypeName = "decimal(10,2)")]
        public decimal PrecioVTA { get; set; }

        [Column("IVA_Porcentaje", TypeName = "decimal(5,2)")]
        public decimal IVA_Porcentaje { get; set; }

        // ============================
        // INVENTARIO
        // ============================
        [Column("Stock")]
        public int Stock { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; }

        // ============================
        // NAVEGACIONES
        // ============================

        [ForeignKey(nameof(ID_Genero))]
        public Genero? Genero { get; set; }


        [ForeignKey(nameof(ID_Referencias))]
        public Referencia? Referencia { get; set; }

        [ForeignKey(nameof(ID_Tallas))]
        public Talla? Talla { get; set; }

        [ForeignKey(nameof(ID_Telas))]
        public Tela? Tela { get; set; }

        [ForeignKey(nameof(ID_Color))]
        public Color? ColorNav { get; set; }
    }
}