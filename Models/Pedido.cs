using InventarioWEB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("pedido")]
public class Pedido
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID_Pedido { get; set; }

    [Required]
    public DateTime Fecha { get; set; } = DateTime.Now;

    // 🔥 AHORA STRING (IGUAL QUE BD)
    [Required]
    [StringLength(50)]
    public string Estado { get; set; } = "Pendiente";

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    // ❌ ELIMINADO: Saldo_Pendiente

    [Required]
    public int ID_Cliente { get; set; }

    public Cliente Cliente { get; set; } = null!;

    // 🔥 OPCIONAL

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalVenta { get; set; }

    public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public ICollection<Abono> Abonos { get; set; } = new List<Abono>();

    public ICollection<Despacho> Despachos { get; set; } = new List<Despacho>();
}