using System;

namespace InventarioWEB.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones necesarias para administrar
    /// la infraestructura de un Tenant.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Crea la infraestructura inicial de un Tenant.
        /// </summary>
        Task CrearTenantAsync(Guid idEmpresa);

        /// <summary>
        /// Crea la base de datos operativa asociada a una empresa.
        /// </summary>
        Task CrearBaseDatosAsync(Guid idEmpresa);

        /// <summary>
        /// Ejecuta las migraciones de Entity Framework Core
        /// sobre la base de datos operativa del Tenant.
        /// </summary>
        Task EjecutarMigracionesAsync(Guid idEmpresa);

        /// <summary>
        /// Obtiene la cadena de conexión de la base de datos
        /// operativa asociada a una empresa.
        /// </summary>
        Task<string?> ObtenerConnectionStringAsync(Guid idEmpresa);

        /// <summary>
        /// Verifica si existe infraestructura operativa
        /// configurada para la empresa.
        /// </summary>
        Task<bool> ExisteTenantAsync(Guid idEmpresa);

        /// <summary>
        /// Resuelve el identificador del Tenant asociado
        /// a una empresa existente.
        /// </summary>
        Task<Guid?> ResolverTenantAsync(Guid idEmpresa);
    }
}