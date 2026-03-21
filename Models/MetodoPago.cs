using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InventarioWEB;
namespace InventarioWEB.Models
{
    /// <summary>
    /// Representa el método de pago disponible (efectivo, tarjeta, transferencia, etc.).
    /// </summary>
    [Table("metodopago")]
    public class MetodoPago
    {
        [Key]
        public int ID_MetodoPago { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Categoria { get; set; }

        public bool Activo { get; set; }

        // ✅ SOLO ESTA RELACIÓN
        public ICollection<Abono> Abonos { get; set; } = new List<Abono>();
    }
}
