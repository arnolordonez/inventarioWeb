using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using InventarioWEB.Reports;

namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador del Módulo de Producción.
    /// 
    /// Responsabilidades:
    /// - Gestionar el inventario operativo derivado de producción.
    /// - Aplicar incrementos de stock (la producción nunca descuenta).
    /// - Ejecutar búsquedas optimizadas con filtros compuestos.
    /// - Gestionar consultas dependientes por género.
    /// - Generar reporte PDF consolidado de producción.
    /// 
    /// Restricciones:
    /// - No crea productos.
    /// - No elimina productos.
    /// - No descuenta inventario.
    /// </summary>
    public class ProduccionController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Constructor del controlador de Producción.
        /// </summary>
        /// <param name="context">
        /// Contexto de base de datos principal utilizado para operaciones
        /// de consulta y actualización del inventario.
        /// </param>
        public ProduccionController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Muestra la vista principal del módulo de Producción.
        /// 
        /// La carga inicial es liviana; los combos dependientes
        /// se cargan dinámicamente vía fetch/AJAX.
        /// </summary>
        /// <returns>Vista Index del módulo Producción.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ProduccionViewModel());
        }

        /// <summary>
        /// Obtiene las referencias asociadas a un género específico.
        /// 
        /// Utilizado para poblar dinámicamente el comboBox de referencias
        /// según el género seleccionado.
        /// </summary>
        /// <param name="idGenero">Identificador del género.</param>
        /// <returns>Listado JSON de referencias filtradas por género.</returns>
        [HttpGet]
        public async Task<IActionResult> ReferenciasPorGenero(int idGenero)
        {
            var data = await _context.Referencias
                .AsNoTracking()
                .Where(r => r.ID_Genero == idGenero)
                .OrderBy(r => r.DescripReferencia)
                .Select(r => new
                {
                    id_Referencias = r.ID_Referencias,
                    nombre = r.DescripReferencia
                })
                .ToListAsync();

            return Json(data);
        }

        /// <summary>
        /// Obtiene las tallas asociadas a un género específico.
        /// 
        /// Permite garantizar coherencia entre género y talla.
        /// </summary>
        /// <param name="idGenero">Identificador del género.</param>
        /// <returns>Listado JSON de tallas filtradas por género.</returns>
        [HttpGet]
        public async Task<IActionResult> TallasPorGenero(int idGenero)
        {
            var data = await _context.Tallas
                .AsNoTracking()
                .Where(t => t.ID_Genero == idGenero)
                .OrderBy(t => t.DescripTalla)
                .Select(t => new
                {
                    id_Tallas = t.ID_Tallas,
                    descripTalla = t.DescripTalla
                })
                .ToListAsync();

            return Json(data);
        }

        /// <summary>
        /// Ejecuta búsqueda dinámica de productos activos con filtros compuestos.
        /// 
        /// Estrategia:
        /// 1. Prioriza búsqueda por clave primaria (ID_Producto).
        /// 2. Si no existe código, aplica filtros combinados.
        /// 3. El género se filtra indirectamente a través de Referencia y Talla.
        /// 
        /// Optimización:
        /// - Uso de AsNoTracking().
        /// - Includes controlados.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Buscar(
            int? codigoProducto,
            int? idGenero,
            int? idReferencia,
            int? idTela,
            int? idTalla,
            int? idColor)
        {
            var query = _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo);

            if (codigoProducto.HasValue)
            {
                query = query.Where(p => p.ID_Producto == codigoProducto.Value);
            }
            else
            {
                if (idReferencia.HasValue)
                    query = query.Where(p => p.ID_Referencias == idReferencia.Value);

                if (idTela.HasValue)
                    query = query.Where(p => p.ID_Telas == idTela.Value);

                if (idTalla.HasValue)
                    query = query.Where(p => p.ID_Tallas == idTalla.Value);

                if (idColor.HasValue)
                    query = query.Where(p => p.ID_Color == idColor.Value);

                if (idGenero.HasValue)
                {
                    query = query.Where(p =>
                        p.Referencia.ID_Genero == idGenero.Value &&
                        p.Talla.ID_Genero == idGenero.Value);
                }
            }

            var resultado = await query
                .Include(p => p.Referencia)
                .Include(p => p.Talla)
                .Include(p => p.Tela)
                .Include(p => p.ColorNav)
                .ToListAsync();

            return PartialView("_ResultadosProduccion", resultado);
        }

        /// <summary>
        /// Actualiza el inventario producto derivado de un proceso de producción.
        /// 
        /// Reglas de negocio:
        /// - La cantidad producida debe ser mayor a cero.
        /// - Los precios no pueden ser negativos.
        /// - El precio de venta no puede ser menor al costo.
        /// - El IVA debe estar entre 0 y 100.
        /// - La producción únicamente incrementa stock.
        /// </summary>
        /// <param name="dto">Objeto de transferencia con datos de producción.</param>
        /// <returns>Resultado HTTP con estado de la operación.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarInventario(ActualizarProduccionDto dto)
        {
            if (dto.CantidadProducida <= 0)
                return BadRequest("La cantidad producida debe ser mayor a cero.");

            if (dto.PrecioCosto < 0 || dto.PrecioVta < 0)
                return BadRequest("Los precios no pueden ser negativos.");

            if (dto.PrecioVta < dto.PrecioCosto)
                return BadRequest("El precio de venta no puede ser menor al costo.");

            if (dto.Iva < 0 || dto.Iva > 100)
                return BadRequest("El IVA debe estar entre 0 y 100.");

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p =>
                    p.ID_Producto == dto.IdProducto && p.Activo);

            if (producto == null)
                return NotFound("Producto no encontrado o inactivo.");

            producto.Stock += dto.CantidadProducida;
            producto.PrecioCosto = dto.PrecioCosto;
            producto.PrecioVTA = dto.PrecioVta;
            producto.IVA_Porcentaje = dto.Iva;

            await _context.SaveChangesAsync();

            return Ok("Inventario actualizado correctamente.");
        }

        /// <summary>
        /// Ejecuta búsqueda paginada server-side.
        /// 
        /// Diseñada para soportar alto volumen de registros (20.000+ productos).
        /// Reduce carga en cliente y mejora performance general.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BuscarPaginado(
            int? codigoProducto,
            int? idGenero,
            int? idReferencia,
            int? idTela,
            int? idTalla,
            int? idColor,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo);

            if (codigoProducto.HasValue)
            {
                query = query.Where(p => p.ID_Producto == codigoProducto.Value);
            }
            else
            {
                if (idReferencia.HasValue)
                    query = query.Where(p => p.ID_Referencias == idReferencia.Value);

                if (idTela.HasValue)
                    query = query.Where(p => p.ID_Telas == idTela.Value);

                if (idTalla.HasValue)
                    query = query.Where(p => p.ID_Tallas == idTalla.Value);

                if (idColor.HasValue)
                    query = query.Where(p => p.ID_Color == idColor.Value);

                if (idGenero.HasValue)
                {
                    query = query.Where(p =>
                        p.Referencia.ID_Genero == idGenero.Value &&
                        p.Talla.ID_Genero == idGenero.Value);
                }
            }

            var totalRegistros = await query.CountAsync();

            var data = await query
                .Include(p => p.Referencia)
                .Include(p => p.Talla)
                .Include(p => p.Tela)
                .Include(p => p.ColorNav)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Json(new
            {
                totalRegistros,
                totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize),
                page,
                pageSize,
                data
            });
        }

        /// <summary>
        /// Genera el reporte consolidado de producción en formato PDF.
        /// 
        /// Características:
        /// - Formato A4 horizontal.
        /// - Incluye fecha de impresión.
        /// - Muestra únicamente productos activos.
        /// - Implementado con QuestPDF.
        /// </summary>
        /// <returns>Archivo PDF descargable.</returns>
        [HttpGet]
        public async Task<IActionResult> GenerarReporteProduccion()
        {
            var data = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Referencia)
                .Include(p => p.Tela)
                .Include(p => p.Talla)
                .Include(p => p.ColorNav)
                .Where(p => p.Activo)
                .ToListAsync();

            var document = new ReporteProduccionPdf(data);
            var pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"ReporteProduccion_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            );
        }

        /// <summary>
        /// DTO utilizado para transportar datos de actualización de producción.
        /// 
        /// Se utiliza exclusivamente en el endpoint de actualización
        /// de inventario.
        /// </summary>
        public class ActualizarProduccionDto
        {
            public int IdProducto { get; set; }
            public int CantidadProducida { get; set; }
            public decimal PrecioCosto { get; set; }
            public decimal PrecioVta { get; set; }
            public decimal Iva { get; set; }
        }
    }
}
