using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Security.Claims;
using Talleres360.Controllers;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Filters;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;
using Xunit;

namespace Talleres360.Tests.Controllers
{
    public class VehiculosControllerTests
    {
        [Fact]
        public void TallerAuthorize_DebeEstarAplicadoEnGetByIdYUpdate()
        {
            var controllerType = typeof(VehiculosController);
            MethodInfo? getByIdMethod = controllerType.GetMethod("GetById");
            MethodInfo? updateMethod = controllerType.GetMethod("Update");

            var getByIdAttr = getByIdMethod?.GetCustomAttributes(typeof(TallerAuthorizeAttribute), false);
            var updateAttr = updateMethod?.GetCustomAttributes(typeof(TallerAuthorizeAttribute), false);

            Assert.NotNull(getByIdAttr);
            Assert.Single(getByIdAttr!);
            Assert.NotNull(updateAttr);
            Assert.Single(updateAttr!);
        }

        [Fact]
        public async Task GetAll_DebeLlamarAlServicio_ConElTallerIdDelToken()
        {
            // ARRANGE
            int tallerIdDelToken = 1;
            var serviceMock = new Mock<IVehiculoService>();
            serviceMock.Setup(x => x.GetAllDetalleByTallerAsync(tallerIdDelToken, 1, 10, null))
                .ReturnsAsync(new PagedResponse<VehiculoDetalle>());

            VehiculosController controller = BuildController(serviceMock, tallerIdDelToken);

            // ACT
            var result = await controller.GetAll(1, 10, null);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            serviceMock.Verify(x => x.GetAllDetalleByTallerAsync(tallerIdDelToken, 1, 10, null), Times.Once);
        }

        [Fact]
        public async Task Add_DebeRetornarConflict_CuandoMatriculaYaExiste()
        {
            // ARRANGE
            var serviceMock = new Mock<IVehiculoService>();
            serviceMock.Setup(x => x.ExistsAsync("ABC123")).ReturnsAsync(true);

            VehiculosController controller = BuildController(serviceMock, 1);
            var vehiculo = new Vehiculo { Matricula = "ABC123" };

            // ACT
            var result = await controller.Add(vehiculo);

            // ASSERT
            Assert.IsType<ConflictObjectResult>(result);
            serviceMock.Verify(x => x.AddAsync(It.IsAny<Vehiculo>()), Times.Never);
        }

        [Fact]
        public async Task Add_DebeRetornarCreatedAtAction_CuandoDatosSonValidos()
        {
            // ARRANGE
            var serviceMock = new Mock<IVehiculoService>();
            serviceMock.Setup(x => x.ExistsAsync("XYZ999")).ReturnsAsync(false);

            VehiculosController controller = BuildController(serviceMock, 1);
            var vehiculo = new Vehiculo { Id = 9, Matricula = "XYZ999" };

            // ACT
            var result = await controller.Add(vehiculo);

            // ASSERT
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", created.ActionName);
            serviceMock.Verify(x => x.AddAsync(It.IsAny<Vehiculo>()), Times.Once);
        }

        [Fact]
        public async Task GetByMatricula_DebeRetornarNotFound_CuandoNoExisteOEsDeOtroTaller()
        {
            // ARRANGE
            var serviceMock = new Mock<IVehiculoService>();
            // Simulamos que el vehículo existe pero es del taller 99 (el usuario es del taller 1)
            serviceMock.Setup(x => x.GetDetalleByMatriculaAsync("OTRO99"))
                .ReturnsAsync(new VehiculoDetalle { TallerId = 99 });

            VehiculosController controller = BuildController(serviceMock, 1);

            // ACT
            var result = await controller.GetByMatricula("OTRO99");

            // ASSERT
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_DebeRetornarBadRequest_CuandoIdNoCoincide()
        {
            // ARRANGE
            var serviceMock = new Mock<IVehiculoService>();
            VehiculosController controller = BuildController(serviceMock, 1);
            var vehiculo = new Vehiculo { Id = 22 };

            // ACT
            var result = await controller.Update(23, vehiculo);

            // ASSERT
            Assert.IsType<BadRequestObjectResult>(result);
            serviceMock.Verify(x => x.UpdateAsync(It.IsAny<Vehiculo>()), Times.Never);
        }

        [Fact]
        public async Task Add_DebeRetornarBadRequest_CuandoVehiculoEsNull()
        {
            // ARRANGE
            var serviceMock = new Mock<IVehiculoService>();
            var controller = BuildController(serviceMock, 1);

            // ACT
            var result = await controller.Add(null!);

            // ASSERT
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("El cuerpo de la petición no puede estar vacío.", badRequest.Value);
            serviceMock.Verify(x => x.AddAsync(It.IsAny<Vehiculo>()), Times.Never);
        }

        [Fact]
        public async Task Update_DebeRetornarBadRequest_CuandoVehiculoEsNull()
        {
            // ARRANGE
            var serviceMock = new Mock<IVehiculoService>();
            var controller = BuildController(serviceMock, 1);

            // ACT
            var result = await controller.Update(23, null!);

            // ASSERT
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Los datos son requeridos.", badRequest.Value);
        }



        private static VehiculosController BuildController(Mock<IVehiculoService> serviceMock, int tallerIdClaim)
        {
            var controller = new VehiculosController(serviceMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim("TallerId", tallerIdClaim.ToString())
                    }, "test"))
                }
            };

            return controller;
        }
    }
}