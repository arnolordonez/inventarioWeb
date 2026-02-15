using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioWEB.ViewModels
{
    public class ProductoCreateViewModel
    {
        // ==========================================================
        // FILTRO PRINCIPAL (NO SE GUARDA EN BD)
        // ==========================================================
        // Sirve únicamente para filtrar Referencias, Tallas y Telas
        public int? ID_Genero { get; set; }

        // ==========================================================
        // CLAVES FORÁNEAS (SE GUARDAN EN PRODUCTOS)
        // ==========================================================
        [Required(ErrorMessage = "Seleccione una referencia.")]
        public int ID_Referencias { get; set; }

        [Required(ErrorMessage = "Seleccione una talla.")]
        public int ID_Tallas { get; set; }

        [Required(ErrorMessage = "Seleccione una tela.")]
        public int ID_Telas { get; set; }

        [Required(ErrorMessage = "Seleccione un color.")]
        public int ID_Color { get; set; }

        // ==========================================================
        // PRECIOS E INVENTARIO
        // ==========================================================
        [Required(ErrorMessage = "Ingrese el precio de costo.")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Precio inválido.")]
        public decimal PrecioCosto { get; set; }

        [Required(ErrorMessage = "Ingrese el precio de venta.")]
        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Precio inválido.")]
        public decimal PrecioVTA { get; set; }

        [Required(ErrorMessage = "Ingrese el IVA.")]
        [Range(typeof(decimal), "0", "100", ErrorMessage = "IVA inválido.")]
        public decimal IVA_Porcentaje { get; set; }

        [Required(ErrorMessage = "Ingrese el stock.")]
        [Range(0, 999999, ErrorMessage = "Stock inválido.")]
        public int Stock { get; set; }

        // ==========================================================
        // CAMPOS GENERADOS AUTOMÁTICAMENTE
        // ==========================================================
        // Se arma desde el controlador (NO se ingresa en la vista)
        public string? Nombre { get; set; }

        // ==========================================================
        // LISTAS PARA DROPDOWNS (UI)
        // ==========================================================
        public IEnumerable<SelectListItem> GenerosLista { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ReferenciasLista { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TallasLista { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TelasLista { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ColoresLista { get; set; } = new List<SelectListItem>();
    }
}
