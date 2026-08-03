namespace InventarioWEB.ViewModels
{
    public class KardexResumenMensualViewModel
    {
        // ==========================================
        // 📅 PERIODO REAL PARA ORDENAMIENTO
        // ==========================================

        public DateTime FechaPeriodo { get; set; }


        // ==========================================
        // 🖥️ TEXTO MOSTRADO EN LA UI
        // ==========================================

        public string Periodo =>
            FechaPeriodo.ToString(
                "MMMM yyyy",
                new System.Globalization.CultureInfo("es-CO")
            );


        // ==========================================
        // 📦 MOVIMIENTOS DEL PERIODO
        // ==========================================

        public int Entradas { get; set; }


        public int Salidas { get; set; }


        // ==========================================
        // 📊 STOCK AL CIERRE DEL MES
        // ==========================================

        public int StockFinal { get; set; }
    }
}