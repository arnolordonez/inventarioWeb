using Microsoft.AspNetCore.Mvc;

namespace InventarioWEB.ViewModels.Catalogos.Referencias
{
    public class ReferenciaDeleteVM
    {
        [HiddenInput]
        public int ID_Referencias { get; set; }

        public string DescripReferencia { get; set; } = string.Empty;

        public string Genero { get; set; } = string.Empty;
    }
}
