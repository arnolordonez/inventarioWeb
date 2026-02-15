using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace InventarioWEB.ViewModels.Catalogos.Telas
{
    public class TelaDeleteVM
    {
        [HiddenInput]
        public int ID_Telas { get; set; }

        [Display(Name = "Descripción")]
        public string DescripTela { get; set; } = string.Empty;
    }
}
