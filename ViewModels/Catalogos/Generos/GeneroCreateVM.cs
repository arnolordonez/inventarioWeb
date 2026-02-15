using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels.Catalogos.Generos
{
    public class GeneroCreateVM
    {
        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Debe tener entre 2 y 100 caracteres.")]
        public string DescripGenero { get; set; } = string.Empty;
    }
}
