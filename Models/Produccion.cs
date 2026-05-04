using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("produccion")]
    public class Produccion
    {
        [Key]
        [Column("ID_Produccion")]
        public int ID_Produccion { get; set; }

        [Required]
        [Column("FechaProduccion")]
        public DateTime FechaProduccion { get; set; }

        [StringLength(255)]
        [Column("Observacion")]
        public string? Observacion { get; set; }

        [StringLength(100)]
        [Column("Usuario")]
        public string? Usuario { get; set; }

        [Required]
        [Column("Activo")]
        public bool Activo { get; set; } = true;

        [Required]
        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        // 🔥 RELACIÓN (UNO A MUCHOS)
        public virtual ICollection<DetalleProduccion> Detalles { get; set; }
            = new List<DetalleProduccion>();
    }
}