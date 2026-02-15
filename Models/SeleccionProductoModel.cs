using System.Collections.Generic;

namespace InventarioWEB.Models
{
    /// <summary>
    /// ViewModel para la selección de atributos de un producto.
    /// Basado estrictamente en la estructura real de la base de datos.
    /// </summary>
    public class SeleccionProductoViewModel
    {
        // =========================
        // Listas para dropdowns
        // =========================

        public List<Genero> Generos { get; set; } = new();
        public List<Talla> Tallas { get; set; } = new();
        public List<Tela> Telas { get; set; } = new();
        public List<Color> Colores { get; set; } = new();
        public List<Referencia> Referencias { get; set; } = new();

        // =========================
        // Valores seleccionados
        // (alineados EXACTO a BD)
        // =========================

        public int? ID_Genero { get; set; }
        public int? ID_Tallas { get; set; }
        public int? ID_Telas { get; set; }
        public int? ID_Color { get; set; }
        public int? ID_Referencias { get; set; }

        // =========================
        // Constructor
        // =========================

        public SeleccionProductoViewModel()
        {
        }
    }
}
