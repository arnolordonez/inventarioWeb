using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioWEB.ViewModels
{
    public class ReferenciaSelectListItem : SelectListItem
    {
        // Esto permitirá filtrar referencias por género
        public int ID_Genero { get; set; }
    }
}
