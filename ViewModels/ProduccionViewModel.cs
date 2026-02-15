using System.Collections.Generic;
using InventarioWEB.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioWEB.ViewModels
{
    public class ProduccionViewModel
    {
        // =====================================================
        // 🔍 FILTROS REALES (SOLO CONTRA PRODUCTOS)
        // =====================================================

        public int? ID_Producto { get; set; }          // Búsqueda directa (PK)
        public int? ID_Referencias { get; set; }
        public int? ID_Tallas { get; set; }
        public int? ID_Telas { get; set; }
        public int? ID_Color { get; set; }

        // =====================================================
        // 📦 DATOS MAESTROS (COMBOBOX – TABLAS PEQUEÑAS)
        // =====================================================

        public List<SelectListItem> Generos { get; set; } = new();
        public List<SelectListItem> Referencias { get; set; } = new();
        public List<SelectListItem> Tallas { get; set; } = new();
        public List<SelectListItem> Telas { get; set; } = new();
        public List<SelectListItem> Colores { get; set; } = new();

        // =====================================================
        // 📊 RESULTADOS
        // =====================================================

        public List<Producto> Productos { get; set; } = new();

        // =====================================================
        // 📈 TOTALES (REPORTES / PRODUCCIÓN)
        // =====================================================

        public int TotalStock { get; set; }
        public decimal TotalCosto { get; set; }
        public decimal TotalVenta { get; set; }
    }
}
