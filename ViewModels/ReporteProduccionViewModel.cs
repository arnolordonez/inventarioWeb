using InventarioWEB.Models;
namespace InventarioWEB.ViewModels
{
    public class ReporteProduccionViewModel
    {
        public Produccion Produccion { get; set; }

        public List<DetalleProduccionReporteVM> Detalles { get; set; }
    }

    public class DetalleProduccionReporteVM
    {
        public int ID_Producto { get; set; }

        public string NombreProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal CostoUnitario { get; set; }

        public decimal PrecioVentaUnitario { get; set; }

        public decimal IVA { get; set; }

        public decimal SubtotalCosto { get; set; }

        public decimal SubtotalVenta { get; set; }
    }
}

