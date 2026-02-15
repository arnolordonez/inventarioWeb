namespace InventarioWEB.ViewModels.Catalogos.Colores
{
    /// <summary>
    /// ViewModel para listado de colores.
    /// </summary>
    public class ColorListadoVM
    {
        public int ID_Color { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}
