using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Talleres360.Interfaces.Seguridad;

namespace Talleres360.API.Filters
{
    public class RequiereSuscripcionActivaAttribute : TypeFilterAttribute
    {
        public RequiereSuscripcionActivaAttribute() : base(typeof(RequiereSuscripcionActivaFilter))
        {
        }

        private class RequiereSuscripcionActivaFilter : IAsyncActionFilter
        {
            private readonly IUserContextService _userContext;
            private readonly ISuscripcionGuardService _suscripcionGuard;

            public RequiereSuscripcionActivaFilter(
                IUserContextService userContext,
                ISuscripcionGuardService suscripcionGuard)
            {
                _userContext = userContext;
                _suscripcionGuard = suscripcionGuard;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                int? tallerId = _userContext.GetTallerId();
                if (tallerId == null)
                {
                    context.Result = new UnauthorizedResult();
                    return; 
                }

                var guard = await _suscripcionGuard.ValidarAccesoEscrituraAsync(tallerId.Value);
                

                if (!guard.PuedeAcceder)
                {
                    context.Result = new ObjectResult(new { mensaje = guard.Mensaje })
                    {
                        StatusCode = StatusCodes.Status402PaymentRequired
                    };
                    return;
                }

                await next();
            }
        }
    }
}