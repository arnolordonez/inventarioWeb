using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    /// <summary>
    /// Catálogo de colores.
    /// Tabla maestra usada directamente en combos del módulo Productos.
    /// Soporta eliminación lógica mediante campo Activo.
    /// </summary>
    [Table("colores")]
    public class Color
    {
        // ==========================================
        // CLAVE PRIMARIA
        // ==========================================
        [Key]
        public int ID_Color { get; set; }

        // ==========================================
        // NOMBRE DEL COLOR
        // ==========================================
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // ==========================================
        // ESTADO (SOFT DELETE)
        // ==========================================
        public bool Activo { get; set; } = true;

        // ==========================================
        // RELACIÓN 1:N CON PRODUCTOS
        // ==========================================
        public ICollection<Producto> Productos { get; set; }
            = new List<Producto>();
    }
}
