using InventarioWEB.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    public class VentaVM
    {
        [Required]
        public int ID_Cliente { get; set; }

        public int? ID_MetodoPago { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalVenta { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AbonoInicial { get; set; }

        public List<DetalleVentaVM> Detalles { get; set; } = new();
    }
}