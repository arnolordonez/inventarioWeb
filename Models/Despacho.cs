using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    public enum TipoDespacho
    {
        Parcial = 1,
        Completo = 2
    }

    public enum EstadoDespacho
    {
        Pendiente = 1,
        Despachado = 2,
        Cancelado = 3
    }

    [Table("despacho")]
    public class Despacho
    {
        [Key]
        public int ID_Despacho { get; set; }

        [Required]
        [Column("ID_Pedido")] // 🔥 Forzamos nombre exacto en BD
        public int ID_Pedido { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public TipoDespacho Tipo { get; set; } = TipoDespacho.Parcial;

        [Required]
        public EstadoDespacho Estado { get; set; } = EstadoDespacho.Pendiente;

        [Column(TypeName = "text")]
        public string? Observacion { get; set; }

        // 🔗 Relación correcta con Pedido
        [ForeignKey("ID_Pedido")]
        public virtual Pedido Pedido { get; set; } = null!;

        public virtual ICollection<DetalleDespacho> Detalles { get; set; } = new List<DetalleDespacho>();
    }
}