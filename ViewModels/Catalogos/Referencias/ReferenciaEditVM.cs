using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioWEB.ViewModels.Catalogos.Referencias
{
    public class ReferenciaEditVM
    {
        [HiddenInput]
        public int ID_Referencias { get; set; }

        [Required(ErrorMessage = "La descripción de la referencia es obligatoria.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Máximo 150 caracteres.")]
        public string? DescripReferencia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un género.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un género válido.")]
        public int ID_Genero { get; set; }

        // SelectList para el dropdown
        public List<SelectListItem> Generos { get; set; } = new();
    }
}
