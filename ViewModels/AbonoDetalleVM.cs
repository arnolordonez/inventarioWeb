namespace InventarioWEB.ViewModels
{
    public class AbonoDetalleVM
    {
        public DateTime Fecha_Abono { get; set; }
       
        public DateTime? FechaRegistro { get; set; } // opcional (auditoría)
                
        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string NumeroRecibo { get; set; } = string.Empty;
    }
}