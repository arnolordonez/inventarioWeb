using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("abono")]
    public class Abono
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Abono { get; set; }

        // =====================================================
        // 🔹 PEDIDO
        // =====================================================
        [Required]
        public int ID_Pedido { get; set; }

        [ForeignKey(nameof(ID_Pedido))]
        public Pedido Pedido { get; set; } = null!;

        // =====================================================
        // 🔹 FECHA DEL ABONO
        // =====================================================
        [Required]
        public DateTime Fecha_Abono { get; set; } = DateTime.Now;

        // =====================================================
        // 🔹 MONTO
        // =====================================================
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        // =====================================================
        // 🔹 MÉTODO DE PAGO
        // =====================================================
        [Required]
        public int ID_MetodoPago { get; set; }

        [ForeignKey(nameof(ID_MetodoPago))]
        public MetodoPago MetodoPago { get; set; } = null!;

        // =====================================================
        // 🔹 ESTADO LÓGICO
        // =====================================================
        [Required]
        public bool Activo { get; set; } = true;

        // =====================================================
        // 🔹 USUARIO
        // =====================================================
        public int? ID_Usuario { get; set; }
                
        [StringLength(200)]
        public string? UsuarioRegistro { get; set; }
        // =====================================================
        // 🔹 OBSERVACIONES
        // =====================================================
        [StringLength(255)]
        public string? Observacion { get; set; }

        // =====================================================
        // 🔹 AUDITORÍA
        // =====================================================
        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // =====================================================
        // 🔹 RECIBO DE CAJA
        // =====================================================

        [StringLength(20)]
        public string? NumeroRecibo { get; set; }

        [StringLength(500)]
       public string? RutaRecibo { get; set; }
    }
}