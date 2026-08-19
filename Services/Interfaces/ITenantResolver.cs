namespace InventarioWEB.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones necesarias para resolver
    /// la infraestructura operativa de un Tenant.
    /// </summary>
    public interface ITenantResolver
    {
        /// <summary>
        /// Obtiene la cadena de conexión correspondiente
        /// al Tenant de la empresa indicada.
        /// </summary>
        Task<string?> ObtenerConnectionStringAsync(Guid idEmpresa);

        /// <summary>
        /// Verifica si existe la base de datos operativa
        /// correspondiente al Tenant.
        /// </summary>
        Task<bool> ExisteTenantAsync(Guid idEmpresa);
    }
}