using System.Collections.Generic;

namespace InventarioWEB.ViewModels
{
    /// <summary>
    /// ViewModel principal de la pantalla Historial de Ventas.
    /// </summary>
    public class HistorialVentasIndexVM
    {
        //===========================================
        // Filtros aplicados
        //===========================================
        public VentaHistorialFiltroVM Filtro { get; set; } = new();

        //===========================================
        // Indicadores
        //===========================================
        public int TotalVentas { get; set; }

        public int TotalPagadas { get; set; }

        public int TotalAbonadas { get; set; }

        public decimal TotalSaldoPendiente { get; set; }

        //===========================================
        // Tabla
        //===========================================
        public List<VentaHistorialVM> Ventas { get; set; } = new();

        //===========================================
        // Paginación
        //===========================================
        public int PaginaActual { get; set; }

        public int TotalPaginas { get; set; }

        public int TotalRegistros { get; set; }

        public int RegistrosPorPagina { get; set; }
    }
}
