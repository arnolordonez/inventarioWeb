using System.Net;

namespace InventarioWEB.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones mediante las cuales el ERP
    /// consulta a la Plataforma Comercial para identificar
    /// una empresa mediante su identificador público.
    /// </summary>
    public interface IPlatformTenantService
    {
        /// <summary>
        /// Resuelve una empresa mediante su SlugEmpresa.
        /// </summary>
        Task<PlatformTenantResolutionResult>
            ResolverEmpresaAsync(
                string slugEmpresa);
    }

    /// <summary>
    /// Resultado de la resolución de una empresa realizada
    /// por la Plataforma Comercial.
    /// </summary>
    public class PlatformTenantResolutionResult
    {
        /// <summary>
        /// Indica si la empresa fue encontrada.
        /// </summary>
        public bool Encontrado { get; init; }

        /// <summary>
        /// Identificador técnico de la empresa.
        /// </summary>
        public Guid? IdEmpresa { get; init; }

        /// <summary>
        /// Identificador público de la empresa.
        /// </summary>
        public string SlugEmpresa { get; init; }
            = string.Empty;

        /// <summary>
        /// Motivo de la respuesta.
        /// </summary>
        public string Motivo { get; init; }
            = string.Empty;

        /// <summary>
        /// Indica si la respuesta HTTP fue correcta.
        /// </summary>
        public bool RespuestaValida { get; init; }

        /// <summary>
        /// Código HTTP devuelto por Platform.
        /// </summary>
        public HttpStatusCode? CodigoHttp { get; init; }
    }
}