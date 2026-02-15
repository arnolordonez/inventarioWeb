using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels.Catalogos.Tallas
{
    public class TallaDeleteVM
    {
        public int ID_Tallas { get; set; }

        [Display(Name = "Descripción")]
        public string DescripTalla { get; set; } = string.Empty;

        [Display(Name = "Género")]
        public string Genero { get; set; } = string.Empty;
    }
}
