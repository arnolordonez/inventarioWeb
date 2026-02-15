using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    /// <summary>
    /// Representa un pedido realizado por un cliente, con su método de pago y totales.
    /// </summary>
    [Table("pedido")]
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Pedido { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? Estado { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Saldo_Pendiente { get; set; }

        // ==========================================================
        // 🔗 RELACIÓN CON CLIENTE
        // ==========================================================
        [Required]
        public int ID_Cliente { get; set; }

        [ForeignKey(nameof(ID_Cliente))]
        public Cliente Cliente { get; set; } = null!;

        // ==========================================================
        // 🔗 RELACIÓN CON MÉTODO DE PAGO
        // ==========================================================
        public int? ID_MetodoPago { get; set; }

        [ForeignKey(nameof(ID_MetodoPago))]
        public MetodoPago? MetodoPago { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalVenta { get; set; }

        // ==========================================================
        // 🔗 RELACIÓN 1:N → Un pedido tiene varios detalles
        // ==========================================================
        public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

        // ==========================================================
        // 🔗 RELACIÓN 1:N → Un pedido puede tener varios abonos
        // ==========================================================
        public ICollection<Abono> Abonos { get; set; } = new List<Abono>();
    }
}
