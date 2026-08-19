using InventarioWEB.Data;

namespace InventarioWEB.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones necesarias para crear
    /// el DbContext correspondiente al Tenant activo.
    ///
    /// Esta fábrica se utiliza en tiempo de ejecución
    /// para conectar el ERP con la base de datos operativa
    /// de la empresa actual.
    /// </summary>
    public interface ITenantDbContextFactory
    {
        /// <summary>
        /// Crea una instancia de TenantDbContext utilizando
        /// la información del Tenant actualmente resuelto.
        /// </summary>
        /// <returns>
        /// Instancia configurada de TenantDbContext.
        /// </returns>
        TenantDbContext CreateDbContext();
    }
}