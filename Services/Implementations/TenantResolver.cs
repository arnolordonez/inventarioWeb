using InventarioWEB.Services.Interfaces;
using MySqlConnector;

namespace InventarioWEB.Services.Implementations
{
    /// <summary>
    /// Resuelve la infraestructura física de un Tenant
    /// sin depender de InventarioWEB.Platform.
    ///
    /// La base de datos se determina mediante:
    /// tenant_{IdEmpresa:N}
    /// </summary>
    public class TenantResolver : ITenantResolver
    {
        private readonly IConfiguration _configuration;

        public TenantResolver(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Obtiene la cadena de conexión de la base de datos
        /// operativa correspondiente a una empresa.
        /// </summary>
        public async Task<string?> ObtenerConnectionStringAsync(
            Guid idEmpresa)
        {
            ValidarIdEmpresa(idEmpresa);

            var serverConnectionString =
                ObtenerConexionServidor();

            var databaseName =
                ObtenerNombreBaseDatos(idEmpresa);

            var existe =
                await ExisteBaseDatosAsync(
                    serverConnectionString,
                    databaseName);

            if (!existe)
            {
                return null;
            }

            var builder =
                new MySqlConnectionStringBuilder(
                    serverConnectionString);

            builder.Database = databaseName;

            return builder.ConnectionString;
        }

        /// <summary>
        /// Determina si existe la base de datos física
        /// correspondiente al Tenant.
        /// </summary>
        public async Task<bool> ExisteTenantAsync(
            Guid idEmpresa)
        {
            ValidarIdEmpresa(idEmpresa);

            var serverConnectionString =
                ObtenerConexionServidor();

            var databaseName =
                ObtenerNombreBaseDatos(idEmpresa);

            return await ExisteBaseDatosAsync(
                serverConnectionString,
                databaseName);
        }

        /// <summary>
        /// Obtiene la conexión al servidor MySQL que contiene
        /// las bases de datos Tenant.
        /// </summary>
        private string ObtenerConexionServidor()
        {
            var connectionString =
                _configuration.GetConnectionString(
                    "ConexionTenantServer");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No se encontró la cadena de conexión " +
                    "'ConexionTenantServer'.");
            }

            return connectionString;
        }

        /// <summary>
        /// Construye el nombre determinístico de la base Tenant.
        /// </summary>
        private static string ObtenerNombreBaseDatos(
            Guid idEmpresa)
        {
            return $"tenant_{idEmpresa:N}";
        }

        /// <summary>
        /// Comprueba la existencia de una base de datos
        /// en el servidor MySQL.
        /// </summary>
        private static async Task<bool> ExisteBaseDatosAsync(
            string serverConnectionString,
            string databaseName)
        {
            var builder =
                new MySqlConnectionStringBuilder(
                    serverConnectionString);

            // La conexión de administración no debe apuntar
            // a una base de datos Tenant específica.
            builder.Database = string.Empty;

            await using var connection =
                new MySqlConnection(
                    builder.ConnectionString);

            await connection.OpenAsync();

            const string sql = """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.SCHEMATA
                WHERE SCHEMA_NAME = @databaseName;
                """;

            await using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@databaseName",
                databaseName);

            var result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Valida el identificador de empresa.
        /// </summary>
        private static void ValidarIdEmpresa(
            Guid idEmpresa)
        {
            if (idEmpresa == Guid.Empty)
            {
                throw new ArgumentException(
                    "El identificador de la empresa es obligatorio.",
                    nameof(idEmpresa));
            }
        }
    }
}