using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("historialinventario")]
    public class HistorialInventario
    {
        public int Id { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // 🔥 OBLIGATORIOS (usar required)
        public required string TipoMovimiento { get; set; }
        public required string DocumentoReferencia { get; set; }

        // 🔥 SKU + descomposición
        public required string SkuArticulo { get; set; }
        public required string Referencia { get; set; }
        public required string Color { get; set; }
        public required string Tela { get; set; }
        public required string Talla { get; set; }

        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }
        public int StockActual { get; set; }

        public int UsuarioId { get; set; }
        public required string UsuarioNombre { get; set; }

        // 🔥 Trazabilidad
        public int? VentaId { get; set; }
        public int? DespachoId { get; set; }

       // public required string Cliente { get; set; }
        public string? Cliente { get; set; }

        // 🔥 OPCIONAL
        public string? Observaciones { get; set; }
    }
}