using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioWEB.ViewModels.Catalogos.Tallas
{
    public class TallaEditVM
    {
        [HiddenInput]
        public int ID_Tallas { get; set; }

        [Required(ErrorMessage = "La descripción de la talla es obligatoria.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string DescripTalla { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un género.")]
        [Display(Name = "Género")]
        public int ID_Genero { get; set; }

        // SelectList para dropdown
        public List<SelectListItem> Generos { get; set; } = new();
    }
}
