using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    [Table("movimiento_inventario")]
    public class MovimientoInventario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Movimiento { get; set; }

        public int ID_Producto { get; set; }

        public string TipoMovimiento { get; set; } = null!;

        public int Cantidad { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string TablaOrigen { get; set; } = null!;

        public int ID_Origen { get; set; }

        public string? Observacion { get; set; }

        public string? Usuario { get; set; }

        // Navegación
        [ForeignKey(nameof(ID_Producto))]
        public Producto Producto { get; set; } = null!;
    }
}