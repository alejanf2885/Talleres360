using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Talleres360.Interfaces.Talleres;
using Talleres360.Interfaces.Seguridad; // Para IUserContextService
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;

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
            if (context == null) throw new ArgumentNullException(nameof(context));

            IUserContextService? userContext = (IUserContextService?)context.HttpContext.RequestServices
                .GetService(typeof(IUserContextService));

            if (userContext == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            int? tallerId = userContext.GetTallerId();

            if (!tallerId.HasValue)
            {
                ApiErrorResponse errorAuth = new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_TOKEN_INVALIDO.ToString(),
                    mensaje: "Identidad del taller no encontrada en la sesión actual."
                );
                context.Result = new ObjectResult(errorAuth) { StatusCode = 401 };
                return;
            }

            if (context.ActionArguments.TryGetValue(_idParamName, out object? idObj) && idObj is int entityId)
            {
                if (entityId <= 0)
                {
                    context.Result = new BadRequestObjectResult(new ApiErrorResponse(
                        ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                        "El identificador del recurso no es válido."));
                    return;
                }

                TRepository? repo = (TRepository?)context.HttpContext.RequestServices
                    .GetService(typeof(TRepository));

                if (repo == null)
                {
                    context.Result = new ObjectResult(new ApiErrorResponse(
                        ErrorCode.SYS_ERROR_GENERICO.ToString(),
                        "Error de configuración: Repositorio de seguridad no disponible."))
                    { StatusCode = 500 };
                    return;
                }

                bool pertenece = await repo.PerteneceATallerAsync(entityId, tallerId.Value);

                if (!pertenece)
                {
                    ApiErrorResponse errorForbidden = new ApiErrorResponse(
                        codigo: ErrorCode.AUTH_ACCESO_DENEGADO.ToString(),
                        mensaje: "Acceso denegado: Este recurso no pertenece a su taller."
                    );
                    context.Result = new ObjectResult(errorForbidden) { StatusCode = 403 };
                    return;
                }
            }

            await next();
        }
    }
}