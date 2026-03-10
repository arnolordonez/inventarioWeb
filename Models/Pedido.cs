using InventarioWEB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.Models
{
    public enum EstadoPedido
    {
        Pendiente = 1,
        ParcialmenteDespachado = 2,
        Despachado = 3,
        Cancelado = 4
    }

    [Table("pedido")]
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID_Pedido")]
        public int ID_Pedido { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Column("Saldo_Pendiente", TypeName = "decimal(10,2)")]
        public decimal Saldo_Pendiente { get; set; }

        [Required]
        [Column("ID_Cliente")]
        public int ID_Cliente { get; set; }

        [ForeignKey("ID_Cliente")]
        public Cliente Cliente { get; set; } = null!;

        [Column("ID_MetodoPago")]
        public int? ID_MetodoPago { get; set; }

        [ForeignKey("ID_MetodoPago")]
        public MetodoPago? MetodoPago { get; set; }

        [Column("TotalVenta", TypeName = "decimal(10,2)")]
        public decimal TotalVenta { get; set; }

        public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

        public ICollection<Abono> Abonos { get; set; } = new List<Abono>();

        [InverseProperty("Pedido")]
        public ICollection<Despacho> Despachos { get; set; } = new List<Despacho>();
    }
}