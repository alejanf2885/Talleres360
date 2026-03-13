using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Filters
{
    public class TallerAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _vehiculoIdParam;

        public TallerAuthorizeAttribute(string vehiculoIdParam = "id")
        {
            _vehiculoIdParam = vehiculoIdParam;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Obtener claim de TallerId
            Claim? claim = context.HttpContext.User.FindFirst("TallerId");
            if (claim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            bool parseOk = int.TryParse(claim.Value, out int usuarioTallerId);
            if (!parseOk)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Verificar si el parámetro existe y es un int
            if (context.ActionArguments.TryGetValue(_vehiculoIdParam, out object? idObj) && idObj is int vehiculoId)
            {
                IVehiculoRepository? repo = (IVehiculoRepository?)context.HttpContext.RequestServices
                    .GetService(typeof(IVehiculoRepository));

                if (repo == null)
                {
                    context.Result = new StatusCodeResult(500);
                    return;
                }

                VehiculoDetalle? vehiculo = await repo.GetDetalleByIdAsync(vehiculoId);

                if (vehiculo == null || vehiculo.TallerId != usuarioTallerId)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}