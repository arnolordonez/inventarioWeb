using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    // =========================================================
    // 📧 AUDITORÍA DE ENVÍO DE CORREOS
    // =========================================================
    // Registra cada intento de envío realizado desde el ERP.
    // Permite conocer:
    // • Qué venta fue enviada.
    // • A quién se envió.
    // • Quién realizó el envío.
    // • Fecha y hora.
    // • Resultado del proceso.
    // =========================================================

    [Table("correo_enviado")]
    public class CorreoEnviado
    {
        // =====================================================
        // CLAVE PRIMARIA
        // =====================================================

        [Key]
        public int IdCorreo { get; set; }

        // =====================================================
        // VENTA RELACIONADA
        // =====================================================

        [Required]
        public int ID_Pedido { get; set; }

        // =====================================================
        // DESTINATARIO
        // =====================================================

        [Required]
        [StringLength(150)]
        public string Destinatario { get; set; } = string.Empty;

        // =====================================================
        // FECHA DEL ENVÍO
        // =====================================================

        public DateTime FechaEnvio { get; set; } = DateTime.Now;

        // =====================================================
        // USUARIO DEL ERP
        // =====================================================

        [Required]
        [StringLength(150)]
        public string Usuario { get; set; } = string.Empty;

        // =====================================================
        // RESULTADO
        // =====================================================

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = string.Empty;

        // =====================================================
        // OBSERVACIONES
        // =====================================================

        [Column(TypeName = "text")]
        public string? Observaciones { get; set; }

        // =====================================================
        // RELACIÓN
        // =====================================================

        [ForeignKey(nameof(ID_Pedido))]
        public Pedido? Pedido { get; set; }
    }
}
