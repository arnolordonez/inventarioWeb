namespace InventarioWEB.DTOs
{
    public class KardexFilterDto
    {
        // =====================================
        // PRODUCTO
        // =====================================
        public int? IdProducto { get; set; }


        // =====================================
        // CATÁLOGOS
        // =====================================

        public int? IdGenero { get; set; }

        public int? IdReferencia { get; set; }

        public int? IdTalla { get; set; }

        public int? IdTela { get; set; }

        public int? IdColor { get; set; }


        // =====================================
        // PERÍODO
        // =====================================

        public string? TipoPeriodo { get; set; }

        public int? Mes { get; set; }

        public int? Anio { get; set; }


        // =====================================
        // RANGO PERSONALIZADO
        // =====================================

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }
    }
}