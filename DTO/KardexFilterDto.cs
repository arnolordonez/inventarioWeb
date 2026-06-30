namespace InventarioWEB.DTOs
{
    public class KardexFilterDto
    {
        // ============================
        // FILTRO PRINCIPAL
        // ============================
        public int? IdProducto { get; set; }

        // ============================
        // FILTROS DE PRODUCTO
        // ============================
        public string? Referencia { get; set; }

        public string? Color { get; set; }

        public string? Tela { get; set; }

        public string? Talla { get; set; }

        // ============================
        // RANGO DE FECHAS
        // ============================
        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }
    }
}