using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Talleres360_front.Services;

namespace Talleres360_front.Filters;

public class AuthRequiredAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string? jwt = context.HttpContext.Session.GetString("jwt");

        if (string.IsNullOrEmpty(jwt))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", new
            {
                returnUrl = context.HttpContext.Request.Path
            });
            return;
        }

        if (!AuthService.EsPerfilConfigurado(context.HttpContext))
        {
            context.Result = new RedirectToActionResult("Setup", "Taller", null);
        }
    }
}
