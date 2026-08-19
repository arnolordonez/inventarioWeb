using InventarioWEB.Data;
using InventarioWEB.Services.Interfaces;

namespace InventarioWEB.Middleware
{
    /// <summary>
    /// Middleware encargado de resolver y proteger
    /// la identidad del Tenant durante la solicitud.
    ///
    /// En esta etapa de desarrollo, el Tenant puede
    /// identificarse mediante el parámetro "empresa".
    /// Una vez autenticado el usuario, el IdEmpresa se
    /// conserva en la sesión para impedir cambios de Tenant.
    /// </summary>
    public class TenantResolverMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolverMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Resuelve el Tenant y verifica que la empresa solicitada
        /// coincida con la empresa establecida en la sesión.
        /// </summary>
        public async Task InvokeAsync(
            HttpContext httpContext,
            ITenantResolver tenantResolver,
            TenantContext tenantContext)
        {
            // ======================================================
            // OBTENER EMPRESA SOLICITADA
            // ======================================================

            var empresaParametro =
                httpContext.Request.Query["empresa"]
                    .FirstOrDefault();

            // ======================================================
            // OBTENER EMPRESA DE LA SESIÓN
            // ======================================================

            var empresaSesion =
                httpContext.Session.GetString("IdEmpresa");

            Guid? idEmpresaSesion = null;

            if (!string.IsNullOrWhiteSpace(empresaSesion))
            {
                if (!Guid.TryParse(
                        empresaSesion,
                        out var guidSesion))
                {
                    httpContext.Session.Clear();
                    tenantContext.Limpiar();

                    httpContext.Response.StatusCode =
                        StatusCodes.Status400BadRequest;

                    await httpContext.Response.WriteAsync(
                        "La identificación de la empresa almacenada en la sesión no es válida.");

                    return;
                }

                idEmpresaSesion = guidSesion;
            }

            // ======================================================
            // RESOLVER IDEMPRESA SOLICITADO
            // ======================================================

            Guid? idEmpresaSolicitada = null;

            if (!string.IsNullOrWhiteSpace(empresaParametro))
            {
                if (!Guid.TryParse(
                        empresaParametro,
                        out var guidParametro))
                {
                    httpContext.Response.StatusCode =
                        StatusCodes.Status400BadRequest;

                    await httpContext.Response.WriteAsync(
                        "El identificador de empresa no tiene un formato válido.");

                    return;
                }

                idEmpresaSolicitada = guidParametro;
            }

            // ======================================================
            // VALIDAR COHERENCIA TENANT / SESIÓN
            // ======================================================

            if (idEmpresaSesion.HasValue &&
                idEmpresaSolicitada.HasValue &&
                idEmpresaSesion.Value != idEmpresaSolicitada.Value)
            {
                httpContext.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                await httpContext.Response.WriteAsync(
                    "La empresa solicitada no coincide con la empresa de la sesión actual.");

                return;
            }

            // ======================================================
            // DETERMINAR TENANT FINAL
            // ======================================================

            var idEmpresa =
                idEmpresaSolicitada ??
                idEmpresaSesion;

            // ======================================================
            // SIN TENANT
            // ======================================================
            // Se permite continuar para rutas públicas,
            // como el acceso inicial al login.
            // Los componentes que requieran Tenant deberán
            // comprobar posteriormente que esté resuelto.

            if (!idEmpresa.HasValue)
            {
                await _next(httpContext);
                return;
            }

            // ======================================================
            // OBTENER CONNECTION STRING
            // ======================================================

            var connectionString =
                await tenantResolver
                    .ObtenerConnectionStringAsync(
                        idEmpresa.Value);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                httpContext.Response.StatusCode =
                    StatusCodes.Status404NotFound;

                await httpContext.Response.WriteAsync(
                    "No existe la infraestructura Tenant correspondiente a la empresa indicada.");

                return;
            }

            // ======================================================
            // ESTABLECER TENANT ACTUAL
            // ======================================================

            tenantContext.EstablecerTenant(
                idEmpresa.Value,
                connectionString);

            // ======================================================
            // CONSERVAR TENANT EN LA SESIÓN
            // ======================================================
            //
            // Si la solicitud llegó con ?empresa=GUID y todavía
            // no existe una empresa asociada a la sesión, se guarda
            // el Tenant seleccionado para las siguientes solicitudes.
            //

            if (!idEmpresaSesion.HasValue)
            {
                httpContext.Session.SetString(
                    "IdEmpresa",
                    idEmpresa.Value.ToString());
            }

            // ======================================================
            // CONTINUAR PIPELINE
            // ======================================================

            await _next(httpContext);
        }
    }
}