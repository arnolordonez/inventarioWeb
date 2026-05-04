using InventarioWEB.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("detalle_despacho")]
[Index(nameof(ID_Despacho), nameof(ID_Detalle), IsUnique = true)]

public class DetalleDespacho
{
    [Key]
    [Column("ID_DetalleDespacho")]
    public int ID_DetalleDespacho { get; set; }

    [Required]
    public int ID_Despacho { get; set; }

    [Required]
    public int ID_Producto { get; set; }

    // 🔥 FALTANTE CRÍTICO
    [Required]
    [Column("ID_Detalle")]
    public int ID_Detalle { get; set; }

    [Required]
    public int Cantidad_Despachada { get; set; }

    [ForeignKey(nameof(ID_Despacho))]
    public virtual Despacho Despacho { get; set; } = null!;

    [ForeignKey(nameof(ID_Producto))]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey(nameof(ID_Detalle))]
    public virtual DetallePedido DetallePedido { get; set; } = null!;
}