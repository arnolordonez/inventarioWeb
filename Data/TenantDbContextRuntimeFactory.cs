using Microsoft.EntityFrameworkCore;

using InventarioWEB.Services.Interfaces;

namespace InventarioWEB.Data
{
    /// <summary>
    /// Fábrica utilizada en tiempo de ejecución para crear
    /// un TenantDbContext conectado a la base de datos
    /// operativa del Tenant actualmente resuelto.
    /// </summary>
    public class TenantDbContextRuntimeFactory : ITenantDbContextFactory
    {
        private readonly TenantContext _tenantContext;

        /// <summary>
        /// Inicializa una nueva instancia de la fábrica.
        /// </summary>
        /// <param name="tenantContext">
        /// Contexto que contiene la información del Tenant actual.
        /// </param>
        public TenantDbContextRuntimeFactory(
            TenantContext tenantContext)
        {
            _tenantContext = tenantContext;
        }

        /// <summary>
        /// Crea un TenantDbContext utilizando la cadena de conexión
        /// correspondiente al Tenant actualmente resuelto.
        /// </summary>
        /// <returns>
        /// Contexto de datos conectado a la base operativa del Tenant.
        /// </returns>
        public TenantDbContext CreateDbContext()
        {
            if (!_tenantContext.EstaResuelto)
            {
                throw new InvalidOperationException(
                    "No existe un Tenant resuelto para la solicitud actual.");
            }

            var connectionString =
                _tenantContext.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "El Tenant actual no tiene una cadena de conexión válida.");
            }

            var optionsBuilder =
                new DbContextOptionsBuilder<TenantDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString));

            return new TenantDbContext(
                optionsBuilder.Options);
        }
    }
}