using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    /// <summary>
    /// Catálogo de telas.
    /// Relación N:M con Referencias.
    /// Eliminación lógica mediante campo Activo.
    /// </summary>
    [Table("telas")]
    public class Tela
    {
        // ============================
        // CLAVE PRIMARIA
        // ============================
        [Key]
        public int ID_Telas { get; set; }

        // ============================
        // DESCRIPCIÓN
        // ============================
        [Required]
        [StringLength(150)]
        public string DescripTela { get; set; } = string.Empty;

        // ============================
        // ELIMINACIÓN LÓGICA
        // ============================
        public bool Activo { get; set; } = true;

        // ============================
        // NAVEGACIÓN N:M
        // ============================
        public ICollection<ReferenciaTela> ReferenciasTelas { get; set; }
            = new List<ReferenciaTela>();
    }
}
