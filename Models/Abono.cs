using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventarioWEB;

namespace InventarioWEB.Models
{
    /// <summary>
    /// /// Representa el pago realizado a un pedido.
    /// Puede corresponder a la cuota inicial o al pago final.
    /// </summary>
    [Table("abono")]
    public class Abono
    {
        [Key]
        public int ID_Abono { get; set; }

        // 🔗 Relación con Pedido (cada abono pertenece a un pedido)
        [Required]
        public int ID_Pedido { get; set; }

        [ForeignKey(nameof(ID_Pedido))]
        public Pedido Pedido { get; set; } = null!;

        // Fecha en que se realizó el abono
        [Required]
       public DateTime Fecha_Abono { get; set; } = DateTime.Now;

        // Monto del abono
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        // 🔗 Relación con el método de pago
        [Required]
        public int ID_MetodoPago { get; set; }

        [ForeignKey(nameof(ID_MetodoPago))]
        public MetodoPago MetodoPago { get; set; } = null!;
    }
}
