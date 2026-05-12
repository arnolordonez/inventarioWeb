using InventarioWEB.Models;

namespace InventarioWEB.ViewModels
{
    public class ProduccionListadoViewModel
    {
        // =====================================================
        // 📄 LISTADO PRINCIPAL
        // =====================================================

        public IReadOnlyList<Produccion> Producciones { get; set; } = [];

        // =====================================================
        // 📄 PAGINACIÓN
        // =====================================================

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; }

        public int TotalRegistros { get; set; }

        public int RegistrosPorPagina { get; set; } = 20;

        public bool TienePaginaAnterior => PaginaActual > 1;

        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

        // =====================================================
        // 🔎 FILTROS
        // =====================================================

        public bool? Activo { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public string? EstadoProduccion { get; set; }

        // =====================================================
        // 📊 INDICADORES / DASHBOARD
        // =====================================================

        public int TotalArticulosDistintos { get; set; }

        public int TotalUnidadesProducidas { get; set; }

        public int TotalProduccionesActivas { get; set; }

        public int TotalProduccionesInactivas { get; set; }

        // =====================================================
        // 💰 TOTALES ECONÓMICOS
        // =====================================================

        public decimal TotalCosto { get; set; }

        public decimal TotalVenta { get; set; }

        public decimal TotalUtilidad { get; set; }
    }
}