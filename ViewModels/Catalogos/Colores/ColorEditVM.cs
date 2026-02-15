using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace InventarioWEB.ViewModels.Catalogos.Colores
{
    public class ColorEditVM
    {
        [HiddenInput]
        public int ID_Color { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Debe tener entre 2 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
