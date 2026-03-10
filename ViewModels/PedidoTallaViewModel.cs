using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    public class PedidoTallaViewModel
    {
        // =========================
        // CONTEXTO DEL PRODUCTO
        // =========================

        [Required]
        public int ID_Genero { get; set; }

        [Required]
        public int ID_Referencia { get; set; }

        [Required]
        public int ID_Producto { get; set; }

        // =========================
        // TALLAS
        // =========================

        public List<TallaCantidadVM> Tallas { get; set; } = new();

        // =========================
        // CÁLCULOS
        // =========================

        public int TotalUnidades => Tallas.Sum(t => t.Cantidad);

        public int TotalDocenas => TotalUnidades / 12;

        public bool EsDocenaValida => TotalUnidades > 0 && TotalUnidades % 12 == 0;
    }

    public class TallaCantidadVM
    {
        [Required]
        public int ID_Talla { get; set; }  // 🔥 CLAVE REAL PARA BD

        public string Descripcion { get; set; } = string.Empty; // Solo UI

        [Range(0, 1000)]
        public int Cantidad { get; set; }
    }
}