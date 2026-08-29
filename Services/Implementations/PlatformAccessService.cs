using System.Net;
using System.Net.Http.Json;
using InventarioWEB.Services.Interfaces;

namespace InventarioWEB.Services.Implementations
{
    /// <summary>
    /// Servicio del ERP encargado de consultar a la Plataforma
    /// Comercial mediante HTTP para validar el derecho de acceso
    /// de una empresa.
    /// </summary>
    public class PlatformAccessService : IPlatformAccessService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlatformAccessService> _logger;

        public PlatformAccessService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PlatformAccessService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Consulta a la Plataforma Comercial la autorización
        /// de una empresa para utilizar el ERP.
        /// </summary>
        public async Task<PlatformAccessResult> ValidarAccesoAsync(
            Guid idEmpresa)
        {

            if (idEmpresa == Guid.Empty)
            {
                return new PlatformAccessResult
                {
                    Permitido = false,
                    IdEmpresa = idEmpresa,
                    Motivo =
                        "El identificador de empresa es obligatorio.",
                    RespuestaValida = false
                };
            }

            var baseUrl =
                _configuration["Platform:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogError(
                    "No está configurada Platform:BaseUrl.");

                return new PlatformAccessResult
                {
                    Permitido = false,
                    IdEmpresa = idEmpresa,
                    Motivo =
                        "No está configurada la dirección de la Plataforma Comercial.",
                    RespuestaValida = false
                };
            }

            if (!Uri.TryCreate(
                    baseUrl,
                    UriKind.Absolute,
                    out var platformUri))
            {
                _logger.LogError(
                    "La configuración Platform:BaseUrl no contiene una URI válida: {BaseUrl}",
                    baseUrl);

                return new PlatformAccessResult
                {
                    Permitido = false,
                    IdEmpresa = idEmpresa,
                    Motivo =
                        "La dirección de la Plataforma Comercial no es válida.",
                    RespuestaValida = false
                };
            }

            try
            {
                var endpoint =
                    $"api/internal/erp-access/{idEmpresa}";

                var response =
                    await _httpClient.GetAsync(endpoint);

                PlatformAccessResult? resultado = null;

                if (response.Content != null)
                {
                    resultado =
                        await response.Content
                            .ReadFromJsonAsync<PlatformAccessResult>();
                }

                if (resultado != null)
                {
                    return new PlatformAccessResult
                    {
                        Permitido = resultado.Permitido,
                        IdEmpresa = resultado.IdEmpresa,
                        Motivo = resultado.Motivo,
                        RespuestaValida = response.IsSuccessStatusCode,
                        CodigoHttp = response.StatusCode
                    };
                }

                return new PlatformAccessResult
                {
                    Permitido = false,
                    IdEmpresa = idEmpresa,
                    Motivo =
                        response.IsSuccessStatusCode
                            ? "La Plataforma no devolvió una respuesta de autorización válida."
                            : $"La Plataforma rechazó la consulta con código HTTP {(int)response.StatusCode}.",
                    RespuestaValida = false,
                    CodigoHttp = response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible comunicarse con la Plataforma Comercial para validar la empresa {IdEmpresa}.",
                    idEmpresa);

                return new PlatformAccessResult
                {
                    Permitido = false,
                    IdEmpresa = idEmpresa,
                    Motivo =
                        "No fue posible comunicarse con la Plataforma Comercial.",
                    RespuestaValida = false
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "La consulta de autorización a la Plataforma excedió el tiempo de espera para la empresa {IdEmpresa}.",
                    idEmpresa);

                return new PlatformAccessResult
                {
                    Permitido = false,
                    IdEmpresa = idEmpresa,
                    Motivo =
                        "La validación comercial excedió el tiempo de espera.",
                    RespuestaValida = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al validar el acceso comercial de la empresa {IdEmpresa}.",
                    idEmpresa);

                return new PlatformAccessResult
                {
                    Permitido = false,
                    IdEmpresa = idEmpresa,
                    Motivo =
                        "No fue posible validar la autorización comercial.",
                    RespuestaValida = false
                };
            }
        }
    }
}