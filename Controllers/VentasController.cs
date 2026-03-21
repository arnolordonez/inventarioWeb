using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace InventarioWEB.Controllers
{
    public class VentasController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        public VentasController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 🔹 INDEX (POS)
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            await CargarFiltrosBaseAsync();
            return View();
        }

        // ==========================================================
        // 🔹 BUSCAR CLIENTES (AUTOCOMPLETE)
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> BuscarClientes(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            term = term.Trim();

            var query = _context.Clientes
                .AsNoTracking()
                .Where(c => c.Activo);

            // 🔥 FILTRO INTELIGENTE
            if (int.TryParse(term, out int idBusqueda))
            {
                // 🔹 Búsqueda por “documento” (ID_Cliente)
                query = query.Where(c => c.ID_Cliente == idBusqueda);
            }
            else
            {
                // 🔹 Búsqueda por texto
                query = query.Where(c =>
                    c.Nombre.Contains(term) ||
                    c.Apellido.Contains(term) ||
                    (c.Nombre + " " + c.Apellido).Contains(term)
                );
            }

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .Select(c => new
                {
                    iD_Cliente = c.ID_Cliente,   // ✅ EXACTO como lo usa tu JS

                    nombreCompleto = (c.Nombre ?? "") + " " + (c.Apellido ?? ""),

                    documento = c.ID_Cliente, // en tu caso la cédula

                    telefono = c.Telefono,
                    correo = c.Correo,
                    ciudad = c.CiudadMunicipio
                })

                .Take(10)
                .ToListAsync();

            return Json(clientes);
        }
        // ==========================================================
        // 🔹 REFERENCIAS POR GÉNERO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerReferenciasPorGenero(int idGenero)
        {
            var data = await _context.Referencias
                .Where(r => r.ID_Genero == idGenero && r.Activo)
                .Select(r => new { r.ID_Referencias, r.DescripReferencia })
                .ToListAsync();

            return Json(data);
        }

        // ==========================================================
        // 🔹 MATRIZ DE TALLAS (CLAVE)
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerMatriz(
            int idGenero,
            int idReferencia,
            int idTela,
            int idColor)
        {
            var productos = await _context.Productos
                .Include(p => p.Talla)
                .Where(p =>
                    p.ID_Genero == idGenero &&
                    p.ID_Referencias == idReferencia &&
                    p.ID_Telas == idTela &&
                    p.ID_Color == idColor &&
                    p.Activo
                )
                .Select(p => new
                {
                    p.ID_Producto,
                    Talla = p.Talla.DescripTalla,
                    p.Stock,
                    p.PrecioVTA
                })
                .OrderBy(p => p.Talla)
                .ToListAsync();

            return Json(productos);
        }

        // ==========================================================
        // 🔹 BÚSQUEDA RÁPIDA POR CÓDIGO
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> BuscarPorCodigo(int idProducto)
        {
            var p = await _context.Productos
                .FirstOrDefaultAsync(x => x.ID_Producto == idProducto);

            if (p == null) return Json(null);

            return Json(new
            {
                p.ID_Producto,
                p.Nombre,
                p.PrecioVTA,
                p.Stock
            });
        }

        // ==========================================================
        // 🔹 GUARDAR VENTA
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] VentaVM venta)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pedido = new Pedido
                {
                    Fecha = DateTime.Now,
                    Estado = venta.AbonoInicial >= venta.TotalVenta ? "Pagado" : "Pendiente",
                    ID_Cliente = venta.ID_Cliente,
                    Total = venta.Total,
                    TotalVenta = venta.TotalVenta
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                var detalles = new List<DetallePedido>();

                foreach (var item in venta.Detalles)
                {
                    if (item.Cantidad <= 0) continue;

                    detalles.Add(new DetallePedido
                    {
                        ID_Pedido = pedido.ID_Pedido,
                        ID_Producto = item.ID_Producto,
                        Cantidad = item.Cantidad,
                        PrecioVenta = item.PrecioVenta,
                        PrecioBase = item.PrecioVenta,
                        Subtotal = item.Cantidad * item.PrecioVenta,
                        Cantidad_Despachada = 0
                    });
                }

                _context.DetallePedidos.AddRange(detalles);
                await _context.SaveChangesAsync();

                var movimientos = detalles.Select(d => new MovimientoInventario
                {
                    ID_Producto = d.ID_Producto,
                    TipoMovimiento = "SALIDA",
                    Cantidad = d.Cantidad * -1,
                    Fecha = DateTime.Now,
                    TablaOrigen = "detalle_pedido",
                    ID_Origen = d.ID_Detalle,
                    Observacion = "Venta",
                    Usuario = "Sistema"
                });

                _context.MovimientoInventarios.AddRange(movimientos);

                if (venta.AbonoInicial > 0 && venta.ID_MetodoPago.HasValue)
                {
                    _context.Abonos.Add(new Abono
                    {
                        ID_Pedido = pedido.ID_Pedido,
                        Fecha_Abono = DateTime.Now,
                        Monto = venta.AbonoInicial,
                        ID_MetodoPago = venta.ID_MetodoPago.Value
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        // ==========================================================
        // 🔹 HISTORIAL
        // ==========================================================
        public async Task<IActionResult> Historial()
        {
            var data = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Abonos)
                .Select(p => new VentaHistorialVM
                {
                    ID_Pedido = p.ID_Pedido,
                    Cliente = p.Cliente.Nombre,
                    Fecha = p.Fecha,
                    TotalVenta = p.TotalVenta,
                    TotalAbonado = p.Abonos.Sum(a => (decimal?)a.Monto) ?? 0,
                    Saldo = p.TotalVenta - (p.Abonos.Sum(a => (decimal?)a.Monto) ?? 0)
                })
                .ToListAsync();

            return View(data);
        }

        // ==========================================================
        // 🔹 REGISTRAR ABONO
        // ==========================================================
        [HttpPost]
        public async Task<IActionResult> RegistrarAbono([FromBody] AbonoVM abonoVM)
        {
            _context.Abonos.Add(new Abono
            {
                ID_Pedido = abonoVM.ID_Pedido,
                Fecha_Abono = DateTime.Now,
                Monto = abonoVM.Monto,
                ID_MetodoPago = abonoVM.ID_MetodoPago
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        

// ==========================================================
// 🔹 CARGAR FILTROS BASE
// ==========================================================
private async Task CargarFiltrosBaseAsync()
    {
        ViewBag.Generos = await _context.Generos
            .Select(g => new SelectListItem
            {
                Value = g.ID_Genero.ToString(),   // ajusta si el nombre cambia
                Text = g.DescripGenero                   // ajusta si el nombre cambia
            })
            .ToListAsync();

        ViewBag.Telas = await _context.Telas
            .Where(t => t.Activo)
            .Select(t => new SelectListItem
            {
                Value = t.ID_Telas.ToString(),
                Text = t.DescripTela
            })
            .ToListAsync();

        ViewBag.Colores = await _context.Colores
            .Where(c => c.Activo)
            .Select(c => new SelectListItem
            {
                Value = c.ID_Color.ToString(),
                Text = c.Nombre
            })
            .ToListAsync();
    }

    /*
    // ==========================================================
    // 🔹 CARGAR FILTROS BASE
    // ==========================================================
    private async Task CargarFiltrosBaseAsync()
    {
        ViewBag.Generos = await _context.Generos.ToListAsync();
        ViewBag.Telas = await _context.Telas.Where(t => t.Activo).ToListAsync();
        ViewBag.Colores = await _context.Colores.Where(c => c.Activo).ToListAsync();
    }
    */
}
}