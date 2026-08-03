using System;
using System.Collections.Generic;
using System.Linq;

namespace InventarioWEB.ViewModels
{
    // ======================================================
    // 🔹 VIEWMODEL PARA CONSULTA DEL DETALLE DE UNA VENTA
    // ======================================================
    // Responsabilidad:
    // • Mostrar el detalle completo de una venta registrada.
    // • Es utilizado por HistorialVentasController.
    // • Alimenta las vistas:
    //      - Detalle.cshtml
    //      - Reporte.cshtml
    // • También sirve como origen de datos para:
    //      - Impresión.
    //      - Exportación a PDF.
    //      - Envío por correo (futuro).
    // ======================================================
    public class VentaDetalleVM
    {
        // ======================================================
        // DATOS GENERALES DE LA VENTA
        // ======================================================
        // ======================================================
        // VALORES ECONÓMICOS DE LA VENTA
        // ======================================================

        // Valor antes de impuestos (Base gravable)
        public decimal Total { get; set; }

        // IVA total del pedido
        public decimal TotalIVA { get; set; }

        // Total final de la venta (Base + IVA)
        public decimal TotalVenta { get; set; }

        // Total abonado por el cliente
        public decimal TotalAbonado { get; set; }

        // Saldo pendiente
        public decimal Saldo => TotalVenta - TotalAbonado;

        // Estado financiero de la venta
        public string Estado => Saldo == 0 ? "Pagado" : "Pendiente";

        // Tipo de venta (CONTADO / CREDITO)
        public string TipoVenta { get; set; } = string.Empty;


        public int ID_Pedido { get; set; }

        public string Cliente { get; set; } = string.Empty;
        public string CorreoCliente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        // ======================================================
        // ESTADO DEL PROCESO COMERCIAL
        // ======================================================

        public string EstadoPedido { get; set; } = string.Empty;

        public string EstadoPago { get; set; } = string.Empty;

        public string EstadoDespacho { get; set; } = string.Empty;

        // Tipo de despacho (PARCIAL / COMPLETO)
        public string TipoDespacho { get; set; } = string.Empty;

        public int? ID_Despacho { get; set; }

        // ======================================================
        // DATOS DERIVADOS PARA REPORTES Y DOCUMENTOS
        // ======================================================
        // Propiedades calculadas que no se almacenan en la base
        // de datos. Se utilizan para la generación de reportes,
        // impresión, exportación a PDF y otros documentos
        // comerciales del ERP.
        // ======================================================

        public string NumeroFactura =>
            ID_Despacho.HasValue
                ? $"FAC-{ID_Despacho}"
                : "NO FACTURADA";

        public DateTime FechaReporte => DateTime.Now;

        public string TituloReporte => "REPORTE DE VENTA";

        public bool TieneAbonos => Abonos.Any();

        public bool EstaDespachada => ID_Despacho.HasValue;
       

        // ======================================================
        // INDICADORES CALCULADOS
        // ======================================================
        // Información obtenida a partir de los productos
        // registrados en la venta.
        // ======================================================

        public int TotalProductos => Productos.Count;

        public int TotalUnidades => Productos.Sum(x => x.Cantidad);

        // ======================================================
        // COLECCIONES RELACIONADAS
        // ======================================================
        // Productos vendidos y pagos realizados asociados
        // a la venta consultada.
        // ======================================================

        public List<DetalleProductoVM> Productos { get; set; } = new();

        public List<AbonoDetalleVM> Abonos { get; set; } = new();
    }
}