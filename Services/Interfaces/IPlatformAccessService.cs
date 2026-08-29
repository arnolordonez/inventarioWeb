using System.Net;

namespace InventarioWEB.Services.Interfaces
{
    /// <summary>
    /// Define las operaciones mediante las cuales el ERP
    /// consulta a la Plataforma Comercial para determinar
    /// si una empresa está autorizada a utilizar el sistema.
    /// </summary>
    public interface IPlatformAccessService
    {
        /// <summary>
        /// Consulta a la Plataforma Comercial si una empresa
        /// posee autorización comercial vigente para utilizar
        /// el ERP.
        /// </summary>
        Task<PlatformAccessResult> ValidarAccesoAsync(
            Guid idEmpresa);
    }

    /// <summary>
    /// Resultado de la validación comercial realizada
    /// por la Plataforma.
    ///
    /// Esta clase pertenece al ERP y evita crear una
    /// dependencia directa hacia InventarioWEB.Platform.
    /// </summary>
    public class PlatformAccessResult
    {
        /// <summary>
        /// Indica si la empresa puede utilizar el ERP.
        /// </summary>
        public bool Permitido { get; init; }

        /// <summary>
        /// Identificador de la empresa validada.
        /// </summary>
        public Guid IdEmpresa { get; init; }

        /// <summary>
        /// Motivo de la decisión comercial.
        /// </summary>
        public string Motivo { get; init; } = string.Empty;

        /// <summary>
        /// Indica si la respuesta fue obtenida correctamente
        /// desde la Plataforma Comercial.
        /// </summary>
        public bool RespuestaValida { get; init; }

        /// <summary>
        /// Código HTTP recibido desde la Plataforma.
        /// </summary>
        public HttpStatusCode? CodigoHttp { get; init; }
    }
}