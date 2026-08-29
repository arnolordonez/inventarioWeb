using System.Net;
using System.Net.Http.Json;
using InventarioWEB.Services.Interfaces;

namespace InventarioWEB.Services.Implementations
{
    /// <summary>
    /// Servicio del ERP encargado de consultar a la Plataforma
    /// Comercial para resolver una empresa mediante su
    /// identificador público SlugEmpresa.
    /// </summary>
    public class PlatformTenantService :
        IPlatformTenantService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlatformTenantService> _logger;

        public PlatformTenantService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PlatformTenantService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Resuelve una empresa mediante su SlugEmpresa.
        /// </summary>
        public async Task<PlatformTenantResolutionResult>
            ResolverEmpresaAsync(
                string slugEmpresa)
        {
            if (string.IsNullOrWhiteSpace(slugEmpresa))
            {
                return new PlatformTenantResolutionResult
                {
                    Encontrado = false,
                    Motivo =
                        "El identificador público de la empresa es obligatorio.",
                    RespuestaValida = false
                };
            }

            slugEmpresa =
                slugEmpresa
                    .Trim()
                    .ToLowerInvariant();

            var baseUrl =
                _configuration["Platform:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogError(
                    "No está configurada Platform:BaseUrl.");

                return new PlatformTenantResolutionResult
                {
                    Encontrado = false,
                    SlugEmpresa = slugEmpresa,
                    Motivo =
                        "No está configurada la dirección de la Plataforma Comercial.",
                    RespuestaValida = false
                };
            }

            try
            {
                var endpoint =
                    $"api/internal/tenant-resolver/{Uri.EscapeDataString(slugEmpresa)}";

                var response =
                    await _httpClient.GetAsync(endpoint);

                PlatformTenantResolutionResult? resultado =
                    null;

                if (response.Content != null)
                {
                    resultado =
                        await response.Content
                            .ReadFromJsonAsync<
                                PlatformTenantResolutionResult>();
                }

                if (resultado != null)
                {
                    return new PlatformTenantResolutionResult
                    {
                        Encontrado = resultado.Encontrado,
                        IdEmpresa = resultado.IdEmpresa,
                        SlugEmpresa =
                            resultado.SlugEmpresa,
                        Motivo = resultado.Motivo,
                        RespuestaValida =
                            response.IsSuccessStatusCode,
                        CodigoHttp =
                            response.StatusCode
                    };
                }

                return new PlatformTenantResolutionResult
                {
                    Encontrado = false,
                    SlugEmpresa = slugEmpresa,
                    Motivo =
                        response.IsSuccessStatusCode
                            ? "La Plataforma no devolvió una respuesta válida para la resolución de empresa."
                            : $"La Plataforma rechazó la consulta con código HTTP {(int)response.StatusCode}.",
                    RespuestaValida = false,
                    CodigoHttp = response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible comunicarse con la Plataforma Comercial para resolver la empresa {SlugEmpresa}.",
                    slugEmpresa);

                return new PlatformTenantResolutionResult
                {
                    Encontrado = false,
                    SlugEmpresa = slugEmpresa,
                    Motivo =
                        "No fue posible comunicarse con la Plataforma Comercial.",
                    RespuestaValida = false
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La resolución de la empresa {SlugEmpresa} excedió el tiempo de espera.",
                    slugEmpresa);

                return new PlatformTenantResolutionResult
                {
                    Encontrado = false,
                    SlugEmpresa = slugEmpresa,
                    Motivo =
                        "La resolución de la empresa excedió el tiempo de espera.",
                    RespuestaValida = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al resolver la empresa {SlugEmpresa}.",
                    slugEmpresa);

                return new PlatformTenantResolutionResult
                {
                    Encontrado = false,
                    SlugEmpresa = slugEmpresa,
                    Motivo =
                        "No fue posible resolver la empresa.",
                    RespuestaValida = false
                };
            }
        }
    }
}
