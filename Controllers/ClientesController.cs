using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Models;
using X.PagedList;
using InventarioWEB.Filters;
using X.PagedList.Extensions;
//using X.PagedList.EF;


namespace InventarioWEB.Controllers
{
    /// <summary>
    /// Controlador responsable de la gestión integral del módulo de Clientes.
    /// Permite listar, crear, editar, consultar detalles, desactivar (soft delete)
    /// y restaurar registros de clientes.
    /// </summary>
    [ValidarSesion]
    public class ClientesController : Controller
    {
        private readonly MovimientoVentasDbContext _context;

        /// <summary>
        /// Constructor del controlador de Clientes.
        /// </summary>
        /// <param name="context">Instancia del contexto de base de datos.</param>
        public ClientesController(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        private bool TieneAcceso()
        {
            var rol = HttpContext.Session.GetString("Rol");

            return rol == "Administrador"
                || rol == "Vendedor";
        }


        /// <summary>
        /// Muestra el listado paginado de clientes.
        /// Permite filtrar por estado (activos/inactivos) y realizar búsquedas por cédula o nombre.
        /// </summary>
        /// <param name="page">Número de página actual.</param>
        /// <param name="soloEliminados">Indica si se deben mostrar únicamente clientes inactivos.</param>
        /// <param name="search">Texto de búsqueda por cédula, nombre o apellido.</param>
        /// <returns>Vista con la lista paginada de clientes.</returns>
        public async Task<IActionResult> Index(
            int? page,
            bool soloEliminados = false,
            string? search = null)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            int pageSize = 10;
            int pageNumber = page ?? 1;

            IQueryable<Cliente> query = _context.Clientes;

            query = soloEliminados
                ? query.Where(c => !c.Activo)
                : query.Where(c => c.Activo);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int cedula))
                    query = query.Where(c => c.ID_Cliente == cedula);
                else
                    query = query.Where(c => c.Nombre.Contains(search) || c.Apellido.Contains(search));
            }

            var clientes = query
            .OrderBy(c => c.Nombre)
            .ToPagedList(pageNumber, pageSize);
            

            ViewBag.SoloEliminados = soloEliminados;
            ViewBag.Search = search;

            return View(clientes);
        }

        /// <summary>
        /// Muestra el detalle de un cliente específico.
        /// </summary>
        /// <param name="id">Identificador único del cliente (cédula).</param>
        /// <returns>Vista con la información del cliente o NotFound si no existe.</returns>
        public async Task<IActionResult> Details(int id)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ID_Cliente == id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        /// <summary>
        /// Muestra el formulario para crear un nuevo cliente.
        /// Inicializa valores por defecto.
        /// </summary>
        /// <returns>Vista de creación de cliente.</returns>
        public async Task<IActionResult> Create()
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            await CargarTiposCliente();
            return View(new Cliente { FechaRegistro = DateTime.Now, Activo = true });
        }

        /// <summary>
        /// Procesa la creación de un nuevo cliente.
        /// Aplica validaciones, genera credenciales por defecto y guarda el registro.
        /// </summary>
        /// <param name="model">Modelo del cliente recibido desde el formulario.</param>
        /// <returns>Redirección al listado si es exitoso o retorna la vista con errores.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente model)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            await CargarTiposCliente();

            if (!ValidarCliente(model, esNuevo: true))
                return View(model);

            model.FechaRegistro = DateTime.Now;
            model.Activo = true;
            model.VIP = model.TipoCliente == "Mayorista";
            model.Salt = Guid.NewGuid().ToString();
            model.HashContrasena = BCrypt.Net.BCrypt.HashPassword("default123" + model.Salt);

            _context.Clientes.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra el formulario de edición de un cliente existente.
        /// </summary>
        /// <param name="id">Identificador del cliente.</param>
        /// <returns>Vista de edición o NotFound si no existe.</returns>
        public async Task<IActionResult> Edit(int id)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            await CargarTiposCliente();
            return View(cliente);
        }

        /// <summary>
        /// Procesa la actualización de un cliente existente.
        /// Aplica validaciones antes de guardar cambios.
        /// </summary>
        /// <param name="id">Identificador del cliente.</param>
        /// <param name="model">Modelo actualizado del cliente.</param>
        /// <returns>Redirección al listado si es exitoso o vista con errores.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente model)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            if (id != model.ID_Cliente) return BadRequest();

            await CargarTiposCliente();

            if (!ValidarCliente(model, esNuevo: false))
                return View(model);

            var clienteDb = await _context.Clientes.FindAsync(id);
            if (clienteDb == null) return NotFound();

            clienteDb.Nombre = model.Nombre;
            clienteDb.Apellido = model.Apellido;
            clienteDb.Telefono = model.Telefono;
            clienteDb.Correo = model.Correo;
            clienteDb.Direccion = model.Direccion;
            clienteDb.CiudadMunicipio = model.CiudadMunicipio;
            clienteDb.TipoCliente = model.TipoCliente;
            clienteDb.Observaciones = model.Observaciones;
            clienteDb.Activo = model.Activo;
            clienteDb.VIP = model.TipoCliente == "Mayorista";

            _context.Clientes.Update(clienteDb);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra la vista de confirmación para desactivar un cliente.
        /// </summary>
        /// <param name="id">Identificador del cliente.</param>
        /// <returns>Vista de confirmación o NotFound si no existe.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TieneAcceso())
                return RedirectToAction("AccesoDenegado", "Auto");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ID_Cliente == id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        /// <summary>
        /// Realiza la desactivación lógica (soft delete) de un cliente.
        /// Cambia el estado Activo a false sin eliminar el registro físicamente.
        /// </summary>
        /// <param name="id">Identificador del cliente.</param>
        /// <returns>Redirección al listado de clientes activos.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            cliente.Activo = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente desactivado.";
            return RedirectToAction(nameof(Index), new { soloEliminados = false });
        }

        /// <summary>
        /// Restaura un cliente previamente desactivado.
        /// </summary>
        /// <param name="id">Identificador del cliente.</param>
        /// <returns>Redirección al listado de clientes inactivos.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            cliente.Activo = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente restaurado.";
            return RedirectToAction(nameof(Index), new { soloEliminados = true });
        }

        /// <summary>
        /// Carga la lista de tipos de cliente desde la base de datos
        /// y los envía a la vista mediante ViewBag.
        /// </summary>
        private async Task CargarTiposCliente()
        {
            ViewBag.Tipos = await _context.TipoClientes
                .Select(t => new SelectListItem { Text = t.Nombre, Value = t.Nombre })
                .ToListAsync();
        }

        /// <summary>
        /// Valida las reglas de negocio del cliente.
        /// Incluye validación de cédula, campos obligatorios y tipo de cliente válido.
        /// </summary>
        /// <param name="model">Modelo de cliente a validar.</param>
        /// <param name="esNuevo">Indica si la validación corresponde a un registro nuevo.</param>
        /// <returns>True si el modelo es válido; en caso contrario False.</returns>
        private bool ValidarCliente(Cliente model, bool esNuevo)
        {
            bool valido = true;

            if (model.ID_Cliente <= 0)
            {
                ModelState.AddModelError("ID_Cliente", "La cédula es obligatoria y debe ser mayor que cero.");
                valido = false;
            }
            else
            {
                string cedula = model.ID_Cliente.ToString();
                if (cedula.Length < 6 || cedula.Length > 10 || cedula.StartsWith("0"))
                {
                    ModelState.AddModelError("ID_Cliente", "La cédula debe tener entre 6 y 10 dígitos y no iniciar en 0.");
                    valido = false;
                }
                else if (esNuevo && _context.Clientes.Any(c => c.ID_Cliente == model.ID_Cliente))
                {
                    ModelState.AddModelError("ID_Cliente", "Ya existe un cliente con esta cédula.");
                    valido = false;
                }
            }

            if (string.IsNullOrWhiteSpace(model.Nombre)) ModelState.AddModelError(nameof(model.Nombre), "El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(model.Apellido)) ModelState.AddModelError(nameof(model.Apellido), "El apellido es obligatorio.");
            if (string.IsNullOrWhiteSpace(model.Correo)) ModelState.AddModelError(nameof(model.Correo), "El correo es obligatorio.");
            if (string.IsNullOrWhiteSpace(model.Telefono)) ModelState.AddModelError(nameof(model.Telefono), "El teléfono es obligatorio.");
            if (string.IsNullOrWhiteSpace(model.Direccion)) ModelState.AddModelError(nameof(model.Direccion), "La dirección es obligatoria.");
            if (string.IsNullOrWhiteSpace(model.CiudadMunicipio)) ModelState.AddModelError(nameof(model.CiudadMunicipio), "La ciudad o municipio es obligatorio.");
            if (!_context.TipoClientes.Any(t => t.Nombre == model.TipoCliente)) ModelState.AddModelError(nameof(model.TipoCliente), "Tipo de cliente inválido.");

            return ModelState.IsValid && valido;
        }
    }
}
