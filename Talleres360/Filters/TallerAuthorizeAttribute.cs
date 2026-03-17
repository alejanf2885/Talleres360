using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Filters
{
    public class TallerAuthorizeAttribute<TRepository> : ActionFilterAttribute
        where TRepository : ITallerRecursoRepository
    {
        private readonly string _idParamName;

        public TallerAuthorizeAttribute(string idParamName = "id")
        {
            _idParamName = idParamName;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Claim? claim = context.HttpContext.User.FindFirst("TallerId");
            if (claim == null)
            {
                context.Result = new UnauthorizedResult(); 
                return;
            }

            bool parseOk = int.TryParse(claim.Value, out int usuarioTallerId);
            if (!parseOk)
            {
                context.Result = new UnauthorizedResult(); 
                return;
            }

            if (context.ActionArguments.TryGetValue(_idParamName, out object? idObj) && idObj is int entityId)
            {
                TRepository? repo = (TRepository?)context.HttpContext.RequestServices
                    .GetService(typeof(TRepository));

                if (repo == null)
                {
                    context.Result = new StatusCodeResult(500);
                    return;
                }

                bool pertenece = await repo.PerteneceATallerAsync(entityId, usuarioTallerId);

                if (!pertenece)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}