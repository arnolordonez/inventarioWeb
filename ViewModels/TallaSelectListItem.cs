using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioWEB.ViewModels
{
    public class TallaSelectListItem : SelectListItem
    {
        // Esto permitirá filtrar tallas por género
        public int ID_Genero { get; set; }
    }
}
