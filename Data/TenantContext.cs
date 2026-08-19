namespace InventarioWEB.Data
{
    /// <summary>
    /// Representa el contexto del Tenant activo durante
    /// una solicitud HTTP del ERP.
    ///
    /// Contiene la información necesaria para identificar
    /// la empresa y la base de datos operativa que
    /// corresponden a la solicitud actual.
    /// </summary>
    public class TenantContext
    {
        /// <summary>
        /// Identificador de la empresa asociada al Tenant actual.
        /// </summary>
        public Guid? IdEmpresa { get; private set; }

        /// <summary>
        /// Cadena de conexión de la base de datos operativa
        /// correspondiente al Tenant actual.
        /// </summary>
        public string? ConnectionString { get; private set; }

        /// <summary>
        /// Indica si existe un Tenant resuelto para la
        /// solicitud actual.
        /// </summary>
        public bool EstaResuelto =>
            IdEmpresa.HasValue &&
            !string.IsNullOrWhiteSpace(ConnectionString);

        /// <summary>
        /// Establece el Tenant correspondiente a la
        /// solicitud actual.
        /// </summary>
        /// <param name="idEmpresa">
        /// Identificador de la empresa.
        /// </param>
        /// <param name="connectionString">
        /// Cadena de conexión de la base de datos operativa.
        /// </param>
        public void EstablecerTenant(
            Guid idEmpresa,
            string connectionString)
        {
            if (idEmpresa == Guid.Empty)
            {
                throw new ArgumentException(
                    "El identificador de la empresa es obligatorio.",
                    nameof(idEmpresa));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(
                    "La cadena de conexión del Tenant es obligatoria.",
                    nameof(connectionString));
            }

            IdEmpresa = idEmpresa;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Limpia la información del Tenant actual.
        /// </summary>
        public void Limpiar()
        {
            IdEmpresa = null;
            ConnectionString = null;
        }
    }
}