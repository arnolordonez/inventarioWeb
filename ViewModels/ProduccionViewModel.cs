using System.Collections.Generic;
using InventarioWEB.Models;

namespace InventarioWEB.ViewModels
{
    public class ProduccionViewModel
    {
        // =====================================================
        // 📄 LISTADO PRINCIPAL (CABECERA PRODUCCIÓN)
        // =====================================================

        public List<Produccion> Producciones { get; set; } = new();

        // =====================================================
        // 📄 PAGINACIÓN
        // =====================================================

        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosPorPagina { get; set; } = 20;

        // =====================================================
        // 🔎 FILTROS OPCIONALES
        // =====================================================

        public bool? Activo { get; set; }

        // =====================================================
        // 📊 TOTALES GENERALES
        // =====================================================

        public int TotalProduccionesActivas { get; set; }
        public int TotalProduccionesInactivas { get; set; }
    }
}