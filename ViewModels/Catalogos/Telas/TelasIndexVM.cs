using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels.Catalogos.Telas
{
    public class TelasIndexVM
    {
        // =========================
        // TELAS ACTIVAS
        // =========================
        public IReadOnlyList<TelaItemVM> Telas { get; init; }
            = new List<TelaItemVM>();

        // =========================
        // TELAS ELIMINADAS (LÓGICAS)
        // =========================
        public IReadOnlyList<TelaItemVM> TelasEliminadas { get; init; }
            = new List<TelaItemVM>();
    }

    public class TelaItemVM
    {
        [Display(Name = "ID")]
        public int ID_Telas { get; init; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Descripción")]
        public string DescripTela { get; init; } = string.Empty;
    }
}
