namespace InventarioWEB.ViewModels.Catalogos.Referencias
{
    public class ReferenciasIndexVM
    {
        public List<ReferenciaItemVM> Referencias { get; set; } = new();
    }

    public class ReferenciaItemVM
    {
        public int ID_Referencias { get; set; }

        public string DescripReferencia { get; set; } = string.Empty;

        // Mostrar género asociado
        public string Genero { get; set; } = string.Empty;

        // 🔹 Nuevo: estado lógico
        public bool Activo { get; set; }
    }
}
