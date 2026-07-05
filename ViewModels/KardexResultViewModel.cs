using System.Collections.Generic;
using System.Linq;

namespace InventarioWEB.ViewModels
{
    public class KardexResultViewModel
    {
        // ================================
        // 📦 MOVIMIENTOS DEL KARDEX
        // ================================
        public List<KardexViewModel> Movimientos { get; set; } = new();

        // ================================
        // 📊 DATOS PARA GRÁFICA
        // ================================
        public List<KardexGraficaViewModel> Grafica { get; set; } = new();

        // ================================
        // 🔢 TOTALES DE MOVIMIENTOS
        // ================================
        public int TotalEntradas { get; set; }
        public int TotalSalidas { get; set; }

        // ================================
        // 📦 STOCK FINAL (FUENTE DE VERDAD)
        // ================================

        public int StockFinal { get; set; }
        
        // ================================
        // 📊 TOTAL REGISTROS
        // ================================
        public int TotalMovimientos =>
            Movimientos?.Count ?? 0;
    }
}