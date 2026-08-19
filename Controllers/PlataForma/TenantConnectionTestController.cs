using InventarioWEB.Data;
using InventarioWEB.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioWEB.Controllers.Plataforma
{
    /// <summary>
    /// Controlador temporal utilizado exclusivamente para comprobar
    /// la resolución dinámica del Tenant y la conexión con su
    /// base de datos operativa.
    /// </summary>
    [Route("TenantConnectionTest")]
    public class TenantConnectionTestController : Controller
    {
        private readonly ITenantDbContextFactory _tenantDbContextFactory;
        private readonly TenantContext _tenantContext;

        public TenantConnectionTestController(
            ITenantDbContextFactory tenantDbContextFactory,
            TenantContext tenantContext)
        {
            _tenantDbContextFactory = tenantDbContextFactory;
            _tenantContext = tenantContext;
        }

        // ==========================================================
        // PRUEBA DE CONEXIÓN DINÁMICA
        // ==========================================================

        /// <summary>
        /// Comprueba la conexión con la base de datos operativa
        /// del Tenant actualmente resuelto.
        ///
        /// La resolución del Tenant se realiza previamente mediante
        /// TenantResolverMiddleware.
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!_tenantContext.EstaResuelto)
                {
                    return Content(
                        "No existe un Tenant resuelto para esta solicitud.");
                }

                await using var context =
                    _tenantDbContextFactory.CreateDbContext();

                var cantidadProductos =
                    await context.Productos
                        .AsNoTracking()
                        .CountAsync();

                var connectionString =
                    context.Database.GetConnectionString();

                return Content(
                    "CONEXIÓN TENANT CORRECTA\n\n" +
                    $"IdEmpresa: {_tenantContext.IdEmpresa}\n\n" +
                    "Base de datos: " +
                    $"{ObtenerNombreBaseDatos(connectionString)}\n\n" +
                    $"Productos registrados: {cantidadProductos}");
            }
            catch (Exception ex)
            {
                return Content(
                    "ERROR EN LA CONEXIÓN DEL TENANT\n\n" +
                    ex.Message +
                    "\n\nDETALLE:\n" +
                    ex);
            }
        }

        // ==========================================================
        // OBTENER NOMBRE DE BASE DE DATOS
        // ==========================================================

        /// <summary>
        /// Obtiene únicamente el nombre de la base de datos
        /// para evitar exponer credenciales de conexión.
        /// </summary>
        private static string ObtenerNombreBaseDatos(
            string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return "No disponible";
            }

            var builder =
                new MySqlConnector.MySqlConnectionStringBuilder(
                    connectionString);

            return builder.Database;
        }
    }
}