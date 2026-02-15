using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("referencias")]
    public class Referencia
    {
        [Key]
        public int ID_Referencias { get; set; }

        [Required]
        [StringLength(150)]
        public string DescripReferencia { get; set; } = string.Empty;

        // ============================
        // RELACIÓN N:1 CON GÉNERO
        // ============================
        [Required]
        public int ID_Genero { get; set; }

        [ForeignKey(nameof(ID_Genero))]
        public Genero Genero { get; set; } = null!;

        // ============================
        // ELIMINACIÓN LÓGICA
        // ============================
        public bool Activo { get; set; } = true;
    }
}
