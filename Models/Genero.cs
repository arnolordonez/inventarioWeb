using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    /// <summary>
    /// Representa el género de los productos o referencias
    /// (Ej: Masculino, Femenino, Unisex)
    /// </summary>
    [Table("genero")]
    public class Genero
    {
        [Key]
        public int ID_Genero { get; set; }

        [Required]
        [StringLength(100)]
        public string DescripGenero { get; set; } = string.Empty;

        // 🔗 Relaciones (solo navegación)
        public ICollection<Talla> Tallas { get; set; } = new List<Talla>();
        public ICollection<Referencia> Referencias { get; set; } = new List<Referencia>();

        // Relación intermedia con telas (si existe en BD)
        public ICollection<ReferenciaTela> ReferenciasTelas { get; set; } = new List<ReferenciaTela>();
    }
}
