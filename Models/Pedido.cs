using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.Models
{
    [Table("pedido")]
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Pedido { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "PENDIENTE";

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required]
        public int ID_Cliente { get; set; }

        public Cliente Cliente { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalVenta { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Saldo { get; set; } = 0;

        [Required]
        [Column("TotalIVA", TypeName = "decimal(10,2)")]
        public decimal TotalIVA { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoVenta { get; set; } = "CONTADO";

        public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

        public ICollection<Abono> Abonos { get; set; } = new List<Abono>();

        public ICollection<Despacho> Despachos { get; set; } = new List<Despacho>();
    }
}