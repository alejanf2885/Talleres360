using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Seguridad; 
using Talleres360.Enums.Errors;

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

                if (!tallerId.HasValue)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                AccesoResult resultado = await _suscripcionGuard.ValidarAccesoEscrituraAsync(tallerId.Value);

                if (!resultado.PuedeAcceder)
                {
                    ApiErrorResponse error = new ApiErrorResponse(
                        codigo: resultado.ErrorCode ?? ErrorCode.SUBS_SIN_PLAN_ACTIVO.ToString(),
                        mensaje: resultado.Mensaje ?? "Su suscripción no permite realizar esta operación."
                    );

                    context.Result = new ObjectResult(error)
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