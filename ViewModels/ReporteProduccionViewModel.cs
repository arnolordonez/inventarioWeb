using InventarioWEB.Models;

namespace InventarioWEB.ViewModels
{
    public class ReporteProduccionViewModel
    {
        // =====================================================
        // CABECERA PRODUCCIÓN
        // =====================================================

        public Produccion Produccion { get; set; } = null!;

        // =====================================================
        // DETALLES
        // =====================================================

        public List<DetalleProduccionReporteVM> Detalles { get; set; } = new();

        // =====================================================
        // INDICADORES GENERALES
        // =====================================================

        public int TotalCantidadProducida { get; set; }

        public decimal TotalCosto { get; set; }

        public decimal TotalVenta { get; set; }

        // =====================================================
        // INDICADORES CONTABLES
        // =====================================================

       

       // public decimal TotalIVA { get; set; }

        // =====================================================
        // RENTABILIDAD
        // =====================================================

        public decimal MargenBrutoEstimado { get; set; }

        // =====================================================
        // TRAZABILIDAD
        // =====================================================

       // public string ConsecutivoDocumento { get; set; } = string.Empty;

       // public DateTime FechaImpresion { get; set; } = DateTime.Now;
    }

    public class DetalleProduccionReporteVM
    {
        // =====================================================
        // PRODUCTO
        // =====================================================

        public int ID_Producto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        // =====================================================
        // PRODUCCIÓN
        // =====================================================

        public int CantidadProducida { get; set; }

        // =====================================================
        // VALORES
        // =====================================================

        public decimal CostoUnitario { get; set; }

        public decimal PrecioVentaUnitario { get; set; }

        public decimal IVA { get; set; }

        // =====================================================
        // SUBTOTALES
        // =====================================================

        public decimal SubtotalCosto { get; set; }

        public decimal SubtotalVenta { get; set; }

        // =====================================================
        // BASE E IMPUESTO
        // =====================================================

        public decimal BaseImponible { get; set; }

        public decimal ValorIVA { get; set; }

        // =====================================================
        // RENTABILIDAD
        // =====================================================

        public decimal MargenBruto
        {
            get
            {
                return SubtotalVenta - SubtotalCosto;
            }
        }
    }
}