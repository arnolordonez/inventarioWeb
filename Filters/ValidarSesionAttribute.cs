using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventarioWEB.Filters
{
    public class ValidarSesionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var usuarioId =
                context.HttpContext.Session.GetString("UsuarioID");

            if (string.IsNullOrEmpty(usuarioId))
            {
                context.Result =
                    new RedirectToActionResult(
                        "Login",
                        "Auto",
                        null);
            }

            base.OnActionExecuting(context);
        }
    }
}