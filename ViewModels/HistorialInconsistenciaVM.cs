namespace InventarioWEB.ViewModels
{
    public class HistorialInconsistenciaVM
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        public string? TipoMovimiento { get; set; }
        public string? Documento { get; set; }

        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }

        public string? Referencia { get; set; }
        public string? Talla { get; set; }
        public string? Color { get; set; }
        public string? Tela { get; set; }

        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockActual { get; set; }

        public string? Usuario { get; set; }

        public int? VentaId { get; set; }
        public int? DespachoId { get; set; }

        public string? Cliente { get; set; }
        public string? Observaciones { get; set; }

        // FLAGS
        public bool SinProducto { get; set; }
        public bool SinReferencia { get; set; }
        public bool SinTalla { get; set; }
        public bool SinColor { get; set; }
        public bool SinTela { get; set; }
        public bool MovimientoInvalido { get; set; }
    }
}