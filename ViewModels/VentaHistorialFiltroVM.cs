using System;
using InventarioWEB.Enums;

namespace InventarioWEB.ViewModels
{
    /// <summary>
    /// Filtros utilizados para consultar el Historial de Ventas.
    /// </summary>
    public class VentaHistorialFiltroVM
    {
        // =====================================================
        // PERÍODO
        // =====================================================

        /// <summary>
        /// Hoy, Semana, Mes, MesAnterior, PorMes, PorFecha, PorAnio.
        /// </summary>
        public PeriodoHistorial Periodo { get; set; } = PeriodoHistorial.Mes;

        // =====================================================
        // FILTRO POR MES
        // =====================================================

        /// <summary>
        /// Mes seleccionado cuando el período es PorMes.
        /// Valores: 1 a 12.
        /// </summary>
        public int? Mes { get; set; }

        /// <summary>
        /// Año correspondiente al filtro PorMes.
        /// </summary>
        public int? AnioMes { get; set; }

        // =====================================================
        // FILTRO POR FECHA
        // =====================================================

        /// <summary>
        /// Fecha inicial.
        /// </summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>
        /// Fecha final.
        /// </summary>
        public DateTime? FechaHasta { get; set; }

        // =====================================================
        // FILTRO POR AÑO
        // =====================================================

        /// <summary>
        /// Año seleccionado cuando el período es PorAnio.
        /// </summary>
        public int? Anio { get; set; }

        // =====================================================
        // FILTROS DEL NEGOCIO
        // =====================================================

        /// <summary>
        /// Todos | PAGADO | ABONADO
        /// </summary>
        public string EstadoPago { get; set; } = "Todos";

        /// <summary>
        /// Todos | DESPACHADO | NO DESPACHADO
        /// </summary>
        public string EstadoDespacho { get; set; } = "Todos";

        /// <summary>
        /// Todas | CONTADO | CREDITO
        /// </summary>
        public string TipoVenta { get; set; } = "Todas";

        // =====================================================
        // BÚSQUEDA GENERAL
        // =====================================================

        /// <summary>
        /// Permite buscar por:
        /// - Número de Pedido
        /// - Número de Factura
        /// - Documento del Cliente
        /// - Nombre o Apellido del Cliente
        /// </summary>
        public string? Buscar { get; set; }

        // =====================================================
        // PAGINACIÓN
        // =====================================================

        /// <summary>
        /// Página actual.
        /// </summary>
        public int Pagina { get; set; } = 1;

        /// <summary>
        /// Registros por página.
        /// </summary>
        public int RegistrosPorPagina { get; set; } = 20;
    }
}