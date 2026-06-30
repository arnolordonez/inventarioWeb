using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("historialinventario")]
    public class HistorialInventario
    {
        public int Id { get; set; }

        // 🔥 Auditoría temporal
        public DateTime FechaRegistro { get; set; }

        // 🔥 Tipo de movimiento (VENTA, DESPACHO, PRODUCCION, AJUSTE)
        public required string TipoMovimiento { get; set; }

        public required string DocumentoReferencia { get; set; }

        // 🔥 Identidad del producto
        public int IdProducto { get; set; }

        // 🔥 Género del producto (Snapshot)
        public int IdGenero { get; set; }

        // 🔥 Snapshot del producto (NO FK)
        public string Referencia { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Tela { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;

        // 🔥 Movimiento
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockActual { get; set; }

        // 🔥 Usuario
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;

        // 🔗 Relaciones opcionales
        public int? VentaId { get; set; }
        public int? DespachoId { get; set; }

        public string? Cliente { get; set; }

        // 📝 Auditoría
        public string? Observaciones { get; set; }
    }
}