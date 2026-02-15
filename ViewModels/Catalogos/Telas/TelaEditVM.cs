using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace InventarioWEB.ViewModels.Catalogos.Telas
{
    public class TelaEditVM
    {
        [HiddenInput]
        public int ID_Telas { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(150,
            MinimumLength = 2,
            ErrorMessage = "Debe tener entre 2 y 150 caracteres.")]
        public string DescripTela { get; set; } = string.Empty;
    }
}
