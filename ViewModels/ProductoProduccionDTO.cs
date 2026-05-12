using System;

namespace InventarioWEB.ViewModels
{
    public class ProductoProduccionDTO
    {
        // =====================================================
        // PRODUCTO
        // =====================================================

        public int ID_Producto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Genero { get; set; } = string.Empty;

        public string Referencia { get; set; } = string.Empty;

        public string Talla { get; set; } = string.Empty;

        public string Tela { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int Stock { get; set; }

        // =====================================================
        // PRECIOS
        // =====================================================

        public decimal PrecioCosto { get; set; }

        public decimal PrecioVTA { get; set; }

        public decimal IVA { get; set; }

        // =====================================================
        // PRODUCCIÓN
        // =====================================================

        public int ID_DetallePedido { get; set; }

        public int CantidadPedido { get; set; }

        public int CantidadProducida { get; set; }

        public int CantidadPendiente { get; set; }
    }
}