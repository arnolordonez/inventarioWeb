using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    // =========================================================
    // TIPO DESPACHO ERP
    // =========================================================

    public enum TipoDespacho
    {
        Parcial = 1,
        Completo = 2
    }

    // =========================================================
    // ESTADO LOGÍSTICO ERP
    // =========================================================

    public enum EstadoDespacho
    {
        Pendiente = 1,
        EnProceso = 2,
        Despachado = 3,
        Cancelado = 4
    }

    // =========================================================
    // DESPACHO ERP
    // =========================================================

    [Table("despacho")]
    public class Despacho
    {
        // =====================================================
        // PK
        // =====================================================

        [Key]
        public int ID_Despacho { get; set; }

        // =====================================================
        // PEDIDO RELACIONADO
        // =====================================================

        [Required]
        [Column("ID_Pedido")]
        public int ID_Pedido { get; set; }

        // =====================================================
        // FECHA CREACIÓN ERP
        // =====================================================

        [Required]
        public DateTime Fecha { get; set; }
            = DateTime.Now;

        // =====================================================
        // TIPO
        // =====================================================

        [Required]
        public TipoDespacho Tipo { get; set; }
            = TipoDespacho.Parcial;

        // =====================================================
        // ESTADO
        // =====================================================

        [Required]
        public EstadoDespacho Estado { get; set; }
            = EstadoDespacho.Pendiente;

        // =====================================================
        // OBSERVACIONES
        // =====================================================

        [Column(TypeName = "text")]
        public string? Observacion { get; set; }

        // =====================================================
        // ERP / TRAZABILIDAD
        // =====================================================

        [StringLength(100)]
        public string UsuarioCreacion { get; set; }
            = "Sistema";

        public DateTime FechaRegistro { get; set; }
            = DateTime.Now;

        // =====================================================
        // CONTROL LOGÍSTICO
        // =====================================================

        public bool Confirmado { get; set; }
            = false;

        public DateTime? FechaConfirmacion { get; set; }

        [StringLength(100)]
        public string? UsuarioConfirmacion { get; set; }

        // =====================================================
        // NAVEGACIÓN
        // =====================================================

        [ForeignKey(nameof(ID_Pedido))]
        public virtual Pedido Pedido { get; set; } = null!;

        public virtual ICollection<DetalleDespacho> Detalles { get; set; }
            = new List<DetalleDespacho>();
    }
}