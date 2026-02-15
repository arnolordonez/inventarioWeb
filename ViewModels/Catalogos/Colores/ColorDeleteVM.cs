using Microsoft.AspNetCore.Mvc;

namespace InventarioWEB.ViewModels.Catalogos.Colores
{
    public class ColorDeleteVM
    {
        [HiddenInput]
        public int ID_Color { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}
