using InventarioWEB.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    public class VentaVM
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente válido")]
        public int ID_Cliente { get; set; }

        public int? ID_MetodoPago { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalVenta { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AbonoInicial { get; set; }

        public List<DetalleVentaVM> Detalles { get; set; } = new();

        // 🔹 NUEVO: tipo de venta (CONTADO o CREDITO)
        [Required]
        public string TipoVenta { get; set; } = "CONTADO"; // valor por defecto
    }
}