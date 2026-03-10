using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Models;
using InventarioWEB.Data;
using InventarioWEB.ViewModels;

namespace InventarioWEB.Controllers
{
    public class DespachoController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        public DespachoController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // LISTADO
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            var despachos = await _context.Despachos
                .Include(d => d.Pedido)
                .OrderByDescending(d => d.Fecha)
                .ToListAsync();

            return View("~/Views/Despachos/Index.cshtml", despachos);
        }

        // ==========================================================
        // DETALLE
        // ==========================================================
        public async Task<IActionResult> Detalle(int id)
        {
            var despacho = await _context.Despachos
                .Include(d => d.Pedido)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Talla)
                .Include(d => d.Detalles)
                    .ThenInclude(dd => dd.Producto)
                        .ThenInclude(p => p.Referencia)
                .FirstOrDefaultAsync(d => d.ID_Despacho == id);

            if (despacho == null)
                return NotFound();

            return View("~/Views/Despachos/Detalle.cshtml", despacho);
        }

        // ==========================================================
        // CREAR DESPACHO (GET)
        // ==========================================================
        // ==========================================================
        // CREAR DESPACHO (GET)
        // ==========================================================
        public async Task<IActionResult> Crear(int idPedido)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.DetallePedidos)
                    .ThenInclude(dp => dp.Producto)
                        .ThenInclude(p => p.Talla)
                .FirstOrDefaultAsync(p => p.ID_Pedido == idPedido);

            if (pedido == null)
                return NotFound();

            if (pedido.Estado == EstadoPedido.Despachado)
                return BadRequest("El pedido ya está completamente despachado");

            // resto del código...
        

        // =========================================
        // HISTÓRICO DESPACHADO
        // =========================================

        var despachado = await _context.DetalleDespachos
                .Join(_context.Despachos,
                      dd => dd.ID_Despacho,
                      d => d.ID_Despacho,
                      (dd, d) => new { dd, d })
                .Where(x => x.d.ID_Pedido == idPedido)
                .GroupBy(x => x.dd.ID_Producto)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Total = g.Sum(x => x.dd.Cantidad_Despachada)
                })
                .ToDictionaryAsync(x => x.ProductoId, x => x.Total);

            var vm = new DespachoTallaViewModel
            {
                ID_Pedido = pedido.ID_Pedido,

                Tallas = pedido.DetallePedidos.Select(dp =>
                {
                    var yaDespachado = despachado.ContainsKey(dp.ID_Producto)
                        ? despachado[dp.ID_Producto]
                        : 0;

                    return new DespachoTallaItemVM
                    {
                        ID_Producto = dp.ID_Producto,
                        Talla = dp.Producto.Talla?.DescripTalla ?? "",
                        CantidadPedida = dp.Cantidad,
                        CantidadDespachada = yaDespachado
                    };
                }).ToList(),

                TotalUnidadesPedido = pedido.DetallePedidos.Sum(x => x.Cantidad)
            };

            return View(vm);
        }

        // ==========================================================
        // CREAR DESPACHO (POST)
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DespachoTallaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================================
                // PEDIDO
                // =========================================

                var pedido = await _context.Pedidos
                    .Include(p => p.DetallePedidos)
                    .FirstOrDefaultAsync(p => p.ID_Pedido == model.ID_Pedido);

                if (pedido == null)
                    throw new Exception("El pedido no existe");

                if (pedido.Estado == EstadoPedido.Despachado)
                    throw new Exception("El pedido ya está cerrado");

                // =========================================
                // PRODUCTOS
                // =========================================

                var productoIds = model.Tallas.Select(t => t.ID_Producto).ToList();

                var productos = await _context.Productos
                    .Where(p => productoIds.Contains(p.ID_Producto))
                    .ToDictionaryAsync(p => p.ID_Producto);

                // =========================================
                // HISTÓRICO DESPACHADO
                // =========================================

                var despachadoHistorico = await _context.DetalleDespachos
                    .Join(_context.Despachos,
                          dd => dd.ID_Despacho,
                          d => d.ID_Despacho,
                          (dd, d) => new { dd, d })
                    .Where(x => x.d.ID_Pedido == model.ID_Pedido)
                    .GroupBy(x => x.dd.ID_Producto)
                    .Select(g => new
                    {
                        ProductoId = g.Key,
                        Total = g.Sum(x => x.dd.Cantidad_Despachada)
                    })
                    .ToDictionaryAsync(x => x.ProductoId, x => x.Total);

                int totalSolicitado = model.TotalUnidades;

                if (totalSolicitado <= 0)
                    throw new Exception("Debe ingresar cantidades");

                if (totalSolicitado % 12 != 0)
                    throw new Exception("El despacho debe ser múltiplo de 12 unidades");

                // =========================================
                // VALIDACIONES
                // =========================================

                foreach (var item in model.Tallas.Where(t => t.Cantidad > 0))
                {
                    if (!productos.TryGetValue(item.ID_Producto, out var producto))
                        throw new Exception($"Producto inválido ID {item.ID_Producto}");

                    if (producto.Stock < item.Cantidad)
                        throw new Exception($"Stock insuficiente para talla {item.Talla}");

                    var pedidoDetalle = pedido.DetallePedidos
                        .FirstOrDefault(d => d.ID_Producto == item.ID_Producto);

                    if (pedidoDetalle == null)
                        throw new Exception($"Producto {item.ID_Producto} no pertenece al pedido");

                    var yaDespachado = despachadoHistorico.ContainsKey(item.ID_Producto)
                        ? despachadoHistorico[item.ID_Producto]
                        : 0;

                    var pendiente = pedidoDetalle.Cantidad - yaDespachado;

                    if (pendiente <= 0)
                        throw new Exception($"La talla {item.Talla} ya está completa");

                    if (item.Cantidad > pendiente)
                        throw new Exception($"Excede pendiente en talla {item.Talla}. Máximo: {pendiente}");
                }

                // =========================================
                // CREAR DESPACHO
                // =========================================

                var despacho = new Despacho
                {
                    ID_Pedido = model.ID_Pedido,
                    Fecha = DateTime.Now,
                    Estado = EstadoDespacho.Despachado
                };

                _context.Despachos.Add(despacho);
                await _context.SaveChangesAsync();


                // =========================================
                // DETALLES + ACTUALIZAR INVENTARIO
                // =========================================
                // =========================================
                // VALIDAR STOCK
                // =========================================

                foreach (var item in model.Tallas.Where(t => t.Cantidad > 0))
                {
                    var producto = productos[item.ID_Producto];

                    if (producto.Stock < item.Cantidad)
                    {
                        ModelState.AddModelError("", $"Stock insuficiente para la talla {item.Talla}");
                        return View(model);
                    }
                }

                // =========================================
                // DETALLES + ACTUALIZAR INVENTARIO
                // =========================================
                foreach (var item in model.Tallas.Where(t => t.Cantidad > 0))
                {
                    var producto = productos[item.ID_Producto];

                    // Registrar detalle del despacho
                    _context.DetalleDespachos.Add(new DetalleDespacho
                    {
                        ID_Despacho = despacho.ID_Despacho,
                        ID_Producto = item.ID_Producto,
                        Cantidad_Despachada = item.Cantidad
                    });

                    // Descontar inventario
                    producto.Stock -= item.Cantidad;
                }

                await _context.SaveChangesAsync();
                // =========================================
                // TIPO DESPACHO
                // =========================================

                var totalPedido = pedido.DetallePedidos.Sum(x => x.Cantidad);

                var totalDespachado = await _context.DetalleDespachos
                    .Join(_context.Despachos,
                          dd => dd.ID_Despacho,
                          d => d.ID_Despacho,
                          (dd, d) => new { dd, d })
                    .Where(x => x.d.ID_Pedido == model.ID_Pedido)
                    .SumAsync(x => (int?)x.dd.Cantidad_Despachada) ?? 0;

                despacho.Tipo = totalDespachado >= totalPedido
                    ? TipoDespacho.Completo
                    : TipoDespacho.Parcial;

                // =========================================
                // ESTADO PEDIDO
                // =========================================
                if (despacho.Tipo == TipoDespacho.Completo)
                {
                    pedido.Estado = EstadoPedido.Despachado;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError("", ex.Message);

                return View(model);
            }
        }
    }
}