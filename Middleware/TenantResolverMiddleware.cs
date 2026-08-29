using InventarioWEB.Data;
using InventarioWEB.Services.Interfaces;

namespace InventarioWEB.Middleware
{
    /// <summary>
    /// Middleware encargado de resolver y proteger
    /// la identidad del Tenant durante la solicitud.
    ///
    /// Durante la transición admite:
    ///
    /// 1. Identificación técnica mediante ?empresa=GUID.
    /// 2. Identificación pública mediante la ruta:
    ///    /e/{slugEmpresa}/...
    ///
    /// El IdEmpresa se conserva en la sesión para evitar
    /// cambios de Tenant durante la navegación.
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
        /// Resuelve el Tenant solicitado y verifica que coincida
        /// con el Tenant previamente establecido en la sesión.
        /// </summary>
        public async Task InvokeAsync(
            HttpContext httpContext,
            ITenantResolver tenantResolver,
            IPlatformTenantService platformTenantService,
            TenantContext tenantContext)
        {
            // ======================================================
            // OBTENER IDENTIFICADOR PÚBLICO DESDE LA RUTA
            // ======================================================
            //
            // Ruta esperada:
            // /e/{slugEmpresa}/...
            //
            // Ejemplo:
            // /e/confecciones-jordano-sas/Auto/Login
            //

            var slugEmpresa =
                httpContext.Request.RouteValues["slugEmpresa"]
                    ?.ToString();

            // ======================================================
            // OBTENER IDENTIFICADOR TÉCNICO LEGACY
            // ======================================================
            //
            // Se mantiene temporalmente para pruebas y transición:
            //
            // ?empresa=4c201f29-...
            //

            var empresaParametro =
                httpContext.Request.Query["empresa"]
                    .FirstOrDefault();

            // ======================================================
            // OBTENER EMPRESA DE LA SESIÓN
            // ======================================================

            var empresaSesion =
                httpContext.Session.GetString("IdEmpresa");

            Guid? idEmpresaSesion = null;

            if (!string.IsNullOrWhiteSpace(
                empresaSesion))
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

                idEmpresaSesion =
                    guidSesion;
            }


            // ======================================================
            // RESOLVER EMPRESA POR SLUG
            // ======================================================

          Guid? idEmpresaPorSlug = null;

            if (!string.IsNullOrWhiteSpace(
                slugEmpresa))
            {
                var resultado =
                    await platformTenantService
                        .ResolverEmpresaAsync(
                            slugEmpresa);

                if (!resultado.RespuestaValida)
                {
                    httpContext.Response.StatusCode =
                        resultado.CodigoHttp.HasValue
                            ? (int)resultado.CodigoHttp.Value
                            : StatusCodes.Status503ServiceUnavailable;

                    await httpContext.Response.WriteAsync(
                        resultado.Motivo);

                    return;
                }

                if (!resultado.Encontrado ||
                    !resultado.IdEmpresa.HasValue)
                {
                    httpContext.Response.StatusCode =
                        StatusCodes.Status404NotFound;

                    await httpContext.Response.WriteAsync(
                        resultado.Motivo);

                    return;
                }

                idEmpresaPorSlug =
                    resultado.IdEmpresa.Value;
            }

            // ======================================================
            // RESOLVER EMPRESA POR GUID
            // ======================================================

            Guid? idEmpresaPorParametro = null;

            if (!string.IsNullOrWhiteSpace(
                empresaParametro))
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

                idEmpresaPorParametro =
                    guidParametro;
            }

            // ======================================================
            // VALIDAR COHERENCIA ENTRE SLUG Y GUID
            // ======================================================

            if (idEmpresaPorSlug.HasValue &&
                idEmpresaPorParametro.HasValue &&
                idEmpresaPorSlug.Value !=
                    idEmpresaPorParametro.Value)
            {
                httpContext.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                await httpContext.Response.WriteAsync(
                    "El identificador público y el identificador técnico corresponden a empresas diferentes.");

                return;
            }

            // ======================================================
            // DETERMINAR EMPRESA SOLICITADA
            // ======================================================

            var idEmpresaSolicitada =
                idEmpresaPorSlug ??
                idEmpresaPorParametro;

            // ======================================================
            // VALIDAR COHERENCIA CON LA SESIÓN
            // ======================================================

            if (idEmpresaSesion.HasValue &&
                idEmpresaSolicitada.HasValue &&
                idEmpresaSesion.Value !=
                    idEmpresaSolicitada.Value)
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
            //
            // Se permite continuar para rutas públicas que
            // todavía no tienen empresa identificada.
            //

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

            if (string.IsNullOrWhiteSpace(
                connectionString))
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
