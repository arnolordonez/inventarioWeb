using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels.Catalogos.Generos
{
    public class GeneroIndexVM
    {
        public IReadOnlyList<GeneroItemVM> Generos { get; set; } = new List<GeneroItemVM>();
    }

    public class GeneroItemVM
    {
        public int ID_Genero { get; set; }

        [Display(Name = "Descripción")]
        public string DescripGenero { get; set; } = string.Empty;
    }
}
