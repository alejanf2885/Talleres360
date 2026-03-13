using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using Talleres360.Filters;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;
using Xunit;

namespace Talleres360.Tests.Filters
{
    public class TallerAuthorizeAttributeTests
    {
        [Theory]
        [InlineData(10, 1, 1, true, false, false)]  // Vehículo pertenece al taller → continua
        [InlineData(10, 1, 2, false, true, false)]  // Vehículo no pertenece → forbid
        [InlineData(99, 1, null, false, true, false)] // Vehículo null → forbid
        [InlineData(10, null, 1, false, true, false)] // Claim null → forbid
        [InlineData(10, 1, 1, true, false, true)] // Parametro distinto → continua sin llamar al repo
        [InlineData(10, 1, 1, false, false, false, true)] // Repo null → status 500
        public async Task TallerAuthorize_Filtros_Completos(
            int vehiculoId,
            int? claimTallerId,
            int? vehiculoTallerId,
            bool esperaContinuar,
            bool esperaForbid,
            bool parametroDistinto = false,
            bool repoNull = false)
        {
            // Arrange
            IVehiculoRepository? repo = null;
            if (!repoNull && !parametroDistinto)
            {
                var repoMock = new Mock<IVehiculoRepository>();
                if (vehiculoTallerId.HasValue)
                {
                    repoMock.Setup(x => x.GetDetalleByIdAsync(vehiculoId))
                        .ReturnsAsync(new VehiculoDetalle { Id = vehiculoId, TallerId = vehiculoTallerId.Value });
                }
                else
                {
                    repoMock.Setup(x => x.GetDetalleByIdAsync(vehiculoId))
                        .ReturnsAsync((VehiculoDetalle?)null);
                }
                repo = repoMock.Object;
            }

            var attribute = parametroDistinto
                ? new TallerAuthorizeAttribute("otroId")
                : new TallerAuthorizeAttribute();

            var context = BuildContext(repo, claimTallerId, vehiculoId);

            bool nextCalled = false;

            // Act
            await attribute.OnActionExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new object()));
            });

            // Assert
            if (repoNull)
            {
                var statusResult = Assert.IsType<StatusCodeResult>(context.Result);
                Assert.Equal(500, statusResult.StatusCode);
                Assert.False(nextCalled);
            }
            else if (esperaForbid)
            {
                Assert.IsType<ForbidResult>(context.Result);
                Assert.False(nextCalled);
            }
            else if (esperaContinuar)
            {
                Assert.Null(context.Result);
                Assert.True(nextCalled);
            }

            // Verificar que el repositorio no se llamó si el parámetro era distinto
            if (parametroDistinto && repo != null)
            {
                var mock = Mock.Get(repo);
                mock.Verify(x => x.GetDetalleByIdAsync(It.IsAny<int>()), Times.Never);
            }
        }

        private static ActionExecutingContext BuildContext(IVehiculoRepository? repository, int? claimTallerId, int vehiculoId)
        {
            var services = new ServiceCollection();
            if (repository != null)
            {
                services.AddSingleton(repository);
            }
            IServiceProvider provider = services.BuildServiceProvider();

            var claims = new List<Claim>();
            if (claimTallerId.HasValue)
            {
                claims.Add(new Claim("TallerId", claimTallerId.Value.ToString()));
            }

            var httpContext = new DefaultHttpContext
            {
                RequestServices = provider,
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var actionArguments = new Dictionary<string, object?> { ["id"] = vehiculoId };

            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, new object());
        }
    }
}