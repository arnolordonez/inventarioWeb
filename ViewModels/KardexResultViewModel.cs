using System.Collections.Generic;

namespace InventarioWEB.ViewModels
{
    public class KardexResultViewModel
    {
        // =========================================================
        // 📊 MOVIMIENTOS DETALLADOS (FUENTE PRINCIPAL)
        // =========================================================
        public List<KardexViewModel> Movimientos { get; set; } = new();

        // =========================================================
        // 📈 GRÁFICA (TIPO FUERTE - EVITA OBJECT ANÓNIMO)
        // =========================================================
        public List<KardexGraficaViewModel> Grafica { get; set; } = new();

        // =========================================================
        // 📦 TOTALES DEL SISTEMA
        // =========================================================
        public int TotalEntradas { get; set; }

        public int TotalSalidas { get; set; }

        // =========================================================
        // 🧠 STOCK FINAL (CRÍTICO ERP)
        // =========================================================
        public int SaldoFinal => Movimientos.Count > 0
            ? Movimientos[^1].Saldo
            : 0;

        // =========================================================
        // 📊 INDICADOR DE MOVIMIENTOS
        // =========================================================
        public int TotalMovimientos => Movimientos?.Count ?? 0;
    }
}