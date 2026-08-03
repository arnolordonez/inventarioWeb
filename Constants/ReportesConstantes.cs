namespace InventarioWEB.Constants
{
    /// <summary>
    /// Constantes institucionales utilizadas por los reportes
    /// oficiales del ERP InventarioWEB.
    /// </summary>
    public static class ReportesConstantes
    {
        // =====================================================
        // EMPRESA
        // =====================================================

        public const string Empresa = "CONFECCIONES INDOMABLE S.A.S.";
        public const string Actividad =
    "Industria Manufacturera de Ropa Interior";

        public const string Nit = "900.123.456-7";

        public const string Ciudad = "Bogotá D.C.";

        public const string Telefono = "300 123 4567";

        public const string Sistema = "ERP InventarioWEB";


        // =====================================================
        // REPORTE KARDEX
        // =====================================================

        public const string CodigoKardex = "REP-KDX-001";

        public const string VersionKardex = "1.0";

        public const string TituloKardex =
            "REPORTE OFICIAL DE KARDEX DE INVENTARIO";

        public const string SubtituloKardex =
            "Histórico de movimientos de entradas, salidas y existencias de inventario.";


        // =====================================================
        // PIE DE REPORTE
        // =====================================================

        public const string LeyendaAuditoria =
            "Documento generado automáticamente por el ERP InventarioWEB. La información corresponde a los movimientos registrados en el Kardex de Inventario y constituye un soporte para procesos de auditoría, control interno y conciliación de inventarios.";
    }
}