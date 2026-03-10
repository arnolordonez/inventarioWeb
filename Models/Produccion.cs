using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("produccion")]
    public class Produccion
    {
        [Key]
        public int ID_Produccion { get; set; }

        public DateTime FechaProduccion { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Observacion { get; set; }

        [StringLength(100)]
        public string? Usuario { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public ICollection<DetalleProduccion> Detalles { get; set; }
            = new List<DetalleProduccion>();
    }
}