using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioWEB.Models
{
    /// <summary>
    /// Define combinaciones válidas de Referencia + Talla + Tela.
    /// NO maneja inventario.
    /// NO representa productos finales.
    /// </summary>
    [Table("referencias_telas")]
    public class ReferenciaTela
    {
        // ============================
        // CLAVES (PK COMPUESTA)
        // ============================
        public int ID_Referencias { get; set; }
        public int ID_Tallas { get; set; }
        public int ID_Telas { get; set; }

        // ============================
        // NAVEGACIÓN
        // ============================
        public Referencia Referencia { get; set; } = null!;
        public Talla Talla { get; set; } = null!;
        public Tela Tela { get; set; } = null!;
    }
}
