using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioWEB.ViewModels
{
    public class ProductosIndexViewModel
    {
        // =====================================================
        // RESULTADOS
        // =====================================================
        public IEnumerable<ProductosIndexItemViewModel> Productos { get; set; }
            = new List<ProductosIndexItemViewModel>();

        // =====================================================
        // PAGINACIÓN (SERVER-SIDE)
        // =====================================================
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public int TotalItems { get; set; }
        public int TotalPages =>
            PageSize > 0
                ? (int)System.Math.Ceiling((double)TotalItems / PageSize)
                : 0;

        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        // =====================================================
        // BÚSQUEDA DIRECTA
        // =====================================================
        public int? ID_Producto { get; set; }

        // =====================================================
        // FILTROS
        // =====================================================
        public int? ID_Genero { get; set; }
        public int? ID_Referencia { get; set; }
        public int? ID_Talla { get; set; }
        public int? ID_Tela { get; set; }
        public string? EstadoFiltro { get; set; }

        // =====================================================
        // LISTAS PARA DROPDOWNS
        // =====================================================
        public IEnumerable<SelectListItem> Generos { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<ReferenciaSelectListItem> Referencias { get; set; }
            = new List<ReferenciaSelectListItem>();

        public IEnumerable<TallaSelectListItem> Tallas { get; set; }
            = new List<TallaSelectListItem>();

        public IEnumerable<SelectListItem> Telas { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> EstadosLista { get; set; }
            = new List<SelectListItem>();
    }
}
