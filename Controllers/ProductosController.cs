using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador principal del módulo Productos.
    /// </summary>
    /// <remarks>
    /// Responsable de la gestión integral de productos:
    /// - Consulta paginada con filtros optimizados
    /// - Creación de productos
    /// - Edición
    /// - Cambio de estado (Activo/Inactivo)
    /// - Visualización de detalles
    ///
    /// Implementa:
    /// ✔ Consultas AsNoTracking para optimización de lectura
    /// ✔ Proyección liviana hacia ViewModels
    /// ✔ Paginación server-side
    /// ✔ Carga controlada de tablas pequeñas
    ///
    /// Este controlador constituye el núcleo funcional del módulo Productos.
    /// </remarks>
    public class ProductosController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia del controlador Productos.
        /// </summary>
        /// <param name="context">
        /// Contexto de base de datos correspondiente al módulo de movimiento y ventas.
        /// </param>
        public ProductosController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        /// <summary>
        /// Muestra la vista principal de productos con filtros y paginación.
        /// </summary>
        /// <param name="model">
        /// Modelo de filtros y configuración de paginación.
        /// </param>
        /// <returns>
        /// Vista Index con lista paginada de productos.
        /// </returns>
        /// <remarks>
        /// Estrategia de rendimiento:
        /// - No carga datos al ingresar (salida temprana).
        /// - Solo ejecuta consulta si existen filtros.
        /// - Proyecta a ViewModel para evitar exponer entidades.
        /// - Implementa paginación server-side.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Index(ProductosIndexViewModel model)
        {
            await CargarFiltros(model);

            model.Productos = new List<ProductosIndexItemViewModel>();

            bool hayFiltros =
                model.ID_Producto.HasValue ||
                model.ID_Genero.HasValue ||
                model.ID_Referencia.HasValue ||
                model.ID_Talla.HasValue ||
                model.ID_Tela.HasValue ||
                !string.IsNullOrEmpty(model.EstadoFiltro);

            if (!hayFiltros)
                return View(model);

            var query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Referencia).ThenInclude(r => r.Genero)
                .Include(p => p.Talla)
                .Include(p => p.Tela)
                .Include(p => p.ColorNav)
                .AsQueryable();

            if (model.ID_Producto.HasValue)
            {
                query = query.Where(p => p.ID_Producto == model.ID_Producto.Value);
            }
            else
            {
                if (model.ID_Referencia.HasValue)
                    query = query.Where(p => p.ID_Referencias == model.ID_Referencia.Value);

                if (model.ID_Talla.HasValue)
                    query = query.Where(p => p.ID_Tallas == model.ID_Talla.Value);

                if (model.ID_Tela.HasValue)
                    query = query.Where(p => p.ID_Telas == model.ID_Tela.Value);

                if (model.ID_Genero.HasValue)
                {
                    query = query.Where(p =>
                        p.Referencia != null &&
                        p.Talla != null &&
                        p.Referencia.ID_Genero == model.ID_Genero.Value &&
                        p.Talla.ID_Genero == model.ID_Genero.Value
                    );
                }
            }

            if (!string.IsNullOrEmpty(model.EstadoFiltro))
            {
                query = model.EstadoFiltro == "A"
                    ? query.Where(p => p.Activo)
                    : query.Where(p => !p.Activo);
            }

            model.Page = model.Page < 1 ? 1 : model.Page;
            model.PageSize = model.PageSize <= 0 ? 20 : model.PageSize;

            model.TotalItems = await query.CountAsync();

            model.Productos = await query
                .OrderBy(p => p.ID_Producto)
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(p => new ProductosIndexItemViewModel
                {
                    ID_Producto = p.ID_Producto,
                    Nombre = p.Nombre ?? "—",
                    Referencia = p.Referencia != null ? p.Referencia.DescripReferencia : "—",
                    Talla = p.Talla != null ? p.Talla.DescripTalla : "—",
                    Tela = p.Tela != null ? p.Tela.DescripTela : "—",
                    Color = p.ColorNav != null ? p.ColorNav.Nombre : "—",
                    PrecioVTA = p.PrecioVTA,
                    IVA_Porcentaje = p.IVA_Porcentaje,
                    Activo = p.Activo
                })
                .ToListAsync();

            return View(model);
        }

        // ============================================================
        // DETALLES
        // ============================================================

        /// <summary>
        /// Muestra el detalle completo de un producto.
        /// </summary>
        /// <param name="id">Identificador primario del producto.</param>
        /// <returns>Vista Detalles o 404 si no existe.</returns>
        /// <remarks>
        /// Se utiliza AsNoTracking ya que es una consulta de solo lectura.
        /// </remarks>
        public async Task<IActionResult> Detalles(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Referencia).ThenInclude(r => r.Genero)
                .Include(p => p.Talla)
                .Include(p => p.Tela)
                .Include(p => p.ColorNav)
                .FirstOrDefaultAsync(p => p.ID_Producto == id);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // ============================================================
        // CREAR
        // ============================================================

        /// <summary>
        /// Muestra el formulario de creación de productos.
        /// </summary>
        public async Task<IActionResult> Crear()
        {
            var model = new ProductoCreateViewModel();
            await CargarListasCrear(model);
            return View(model);
        }

        /// <summary>
        /// Registra un nuevo producto en el sistema.
        /// </summary>
        /// <param name="model">Modelo de creación.</param>
        /// <remarks>
        /// Regla de negocio:
        /// El precio de venta debe ser mayor que el precio de costo.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProductoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasCrear(model);
                return View(model);
            }

            if (model.PrecioVTA <= model.PrecioCosto)
            {
                ModelState.AddModelError(nameof(model.PrecioVTA),
                    "El precio de venta debe ser mayor al costo.");
                await CargarListasCrear(model);
                return View(model);
            }

            _context.Productos.Add(new Producto
            {
                Nombre = model.Nombre!,
                PrecioCosto = model.PrecioCosto,
                PrecioVTA = model.PrecioVTA,
                IVA_Porcentaje = model.IVA_Porcentaje,
                Stock = model.Stock,
                ID_Referencias = model.ID_Referencias,
                ID_Tallas = model.ID_Tallas,
                ID_Telas = model.ID_Telas,
                ID_Color = model.ID_Color,
                Activo = true
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // EDITAR
        // ============================================================

        /// <summary>
        /// Muestra la vista de edición de un producto.
        /// </summary>
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            await CargarListasEditar();
            return View(producto);
        }

        /// <summary>
        /// Actualiza la información de un producto existente.
        /// </summary>
        /// <remarks>
        /// Se reutiliza la validación de regla de negocio del precio.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasEditar();
                return View(model);
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            if (model.PrecioVTA <= model.PrecioCosto)
            {
                ModelState.AddModelError("", "El precio de venta debe ser mayor al costo.");
                await CargarListasEditar();
                return View(model);
            }

            _context.Entry(producto).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // CAMBIO DE ESTADO
        // ============================================================

        /// <summary>
        /// Alterna el estado Activo/Inactivo de un producto.
        /// </summary>
        /// <param name="id">Identificador del producto.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Activo = !producto.Activo;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Estado del producto actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // AJAX: REFERENCIAS POR GÉNERO
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ReferenciasPorGenero(int id)
        {
            var referencias = await _context.Referencias
                .Where(r => r.ID_Genero == id && r.Activo)
                .Select(r => new
                {
                    r.ID_Referencias,
                    r.DescripReferencia
                })
                .ToListAsync();

            return Json(referencias);
        }

        // ============================================================
        // AJAX: TALLAS POR REFERENCIA
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> TallasPorGenero(int id)
        {
            var tallas = await _context.Tallas
                .Where(t => t.ID_Genero == id && t.Activo)
                .Select(t => new
                {
                    t.ID_Tallas,
                    t.DescripTalla
                })
                .ToListAsync();

            return Json(tallas);
        }
        // ============================================================
        // MÉTODOS PRIVADOS DE APOYO
        // ============================================================

        /// <summary>
        /// Carga los filtros del Index (tablas pequeñas).
        /// </summary>
        private async Task CargarFiltros(ProductosIndexViewModel model)
        {
            model.Generos = await _context.Generos
                .Select(g => new SelectListItem
                {
                    Value = g.ID_Genero.ToString(),
                    Text = g.DescripGenero
                })
                .ToListAsync();

            model.Referencias = await _context.Referencias
                .Select(r => new ReferenciaSelectListItem
                {
                    Value = r.ID_Referencias.ToString(),
                    Text = r.DescripReferencia,
                    ID_Genero = r.ID_Genero
                })
                .ToListAsync();

            model.Tallas = await _context.Tallas
                .Select(t => new TallaSelectListItem
                {
                    Value = t.ID_Tallas.ToString(),
                    Text = t.DescripTalla,
                    ID_Genero = t.ID_Genero
                })
                .ToListAsync();

            model.Telas = await _context.Telas
                .Select(t => new SelectListItem
                {
                    Value = t.ID_Telas.ToString(),
                    Text = t.DescripTela
                })
                .ToListAsync();

            model.EstadosLista = new List<SelectListItem>
            {
                new() { Value = "A", Text = "Activos" },
                new() { Value = "I", Text = "Inactivos" }
            };
        }

        /// <summary>
        /// Carga las listas necesarias para la vista Crear.
        /// </summary>
        private async Task CargarListasCrear(ProductoCreateViewModel model)
        {
            model.TelasLista = await _context.Telas
                .Select(t => new SelectListItem { Value = t.ID_Telas.ToString(), Text = t.DescripTela })
                .ToListAsync();

            model.ColoresLista = await _context.Colores
                .Select(c => new SelectListItem { Value = c.ID_Color.ToString(), Text = c.Nombre })
                .ToListAsync();

            model.GenerosLista = await _context.Generos
                .Select(g => new SelectListItem { Value = g.ID_Genero.ToString(), Text = g.DescripGenero })
                .ToListAsync();

            // ❌ NO precargar dependientes
            model.ReferenciasLista = new List<SelectListItem>();
            model.TallasLista = new List<SelectListItem>();
        }

        /// <summary>
        /// Carga listas necesarias para la vista Editar.
        /// </summary>
        private async Task CargarListasEditar()
        {
            ViewBag.Telas = await _context.Telas
                .Select(t => new SelectListItem { Value = t.ID_Telas.ToString(), Text = t.DescripTela })
                .ToListAsync();

            ViewBag.Colores = await _context.Colores
                .Select(c => new SelectListItem { Value = c.ID_Color.ToString(), Text = c.Nombre })
                .ToListAsync();
        }
    }
}
