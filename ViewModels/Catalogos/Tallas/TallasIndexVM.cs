using System.Collections.Generic;

namespace InventarioWEB.ViewModels.Catalogos.Tallas
{
    public class TallasIndexVM
    {
        public List<TallaItemVM> Tallas { get; set; } = new();
    }

    public class TallaItemVM
    {
        public int ID_Tallas { get; set; }

        public string DescripTalla { get; set; } = string.Empty;

        // Nombre del género asociado (solo lectura para mostrar en Index)
        public string Genero { get; set; } = string.Empty;
    }
}
