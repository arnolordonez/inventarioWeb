namespace InventarioWEB.ViewModels
{
    public class ProduccionPedidoDashboardVM
    {
        // =====================================================
        // 📋 LISTADO PRINCIPAL
        // =====================================================

        public IReadOnlyList<ProduccionPedidoItemVM> Pedidos
        { get; set; } = new List<ProduccionPedidoItemVM>();

        // =====================================================
        // 📄 PAGINACIÓN
        // =====================================================

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; }

        public int TotalRegistros { get; set; }

        public int RegistrosPorPagina { get; set; } = 20;

        public bool TienePaginaAnterior
            => PaginaActual > 1;

        public bool TienePaginaSiguiente
            => PaginaActual < TotalPaginas;

        // =====================================================
        // 📊 DASHBOARD OPERACIONAL
        // =====================================================

        public int TotalPedidosPendientes { get; set; }

        public int TotalPedidosEnProduccion { get; set; }

        public int TotalPedidosCompletados { get; set; }

        public int TotalUnidadesPendientes { get; set; }

        public int TotalUnidadesProducidas { get; set; }
    }
}