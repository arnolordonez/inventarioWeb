using System.Collections.Generic;

namespace InventarioWEB.ViewModels.Catalogos.Colores
{
    public class ColoresIndexVM
    {
        public IReadOnlyList<ColorListadoVM> Colores { get; set; }
            = new List<ColorListadoVM>();

        public IReadOnlyList<ColorListadoVM> ColoresInactivos { get; set; }
            = new List<ColorListadoVM>();
    }
       
}
