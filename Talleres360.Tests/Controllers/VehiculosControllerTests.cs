using Microsoft.AspNetCore.Mvc;
using Moq;
using Talleres360.API.Controllers;
using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Tests.Controllers
{
    public class VehiculosControllerTests
    {
        private readonly Mock<IVehiculoService> _vehiculoServiceMock;
        private readonly Mock<IUserContextService> _userContextMock;
        private readonly VehiculosController _controller;

        public VehiculosControllerTests()
        {
            _vehiculoServiceMock = new Mock<IVehiculoService>();
            _userContextMock = new Mock<IUserContextService>();

            _controller = new VehiculosController(
                _vehiculoServiceMock.Object,
                _userContextMock.Object);
        }

        [Fact]
        public async Task GetAll_SinTallerId_DebeRetornar401()
        {
            // Arrange
            var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
            _userContextMock.Setup(x => x.GetTallerId()).Returns((int?)null);

            // Act
            var result = await _controller.GetAll(pagination, "1234ABC", 1, 2);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            _vehiculoServiceMock.Verify(
                x => x.GetAllDetalleByTallerPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<VehiculoFiltroDto?>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAll_ConFiltros_DebeRetornar200()
        {
            // Arrange
            var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var paged = new PagedResponse<VehiculoDetalle>
            {
                Data = [new VehiculoDetalle { Id = 1, Matricula = "1234ABC", MarcaId = 1, ModeloId = 2 }],
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _vehiculoServiceMock
                .Setup(x => x.GetAllDetalleByTallerPagedAsync(
                    1,
                    1,
                    10,
                    It.Is<VehiculoFiltroDto>(f =>
                        f.Matricula == "1234ABC" &&
                        f.MarcaId == 1 &&
                        f.ModeloId == 2)))
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.GetAll(pagination, "1234ABC", 1, 2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetById_Encontrado_DebeRetornar200()
        {
            // Arrange
            var detalle = new VehiculoDetalle { Id = 5, Matricula = "1111BBB" };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _vehiculoServiceMock
                .Setup(x => x.GetDetalleByIdAsync(1, 5))
                .ReturnsAsync(ServiceResult<VehiculoDetalle>.Ok(detalle));

            // Act
            var result = await _controller.GetById(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetById_NoEncontrado_DebeRetornar404()
        {
            // Arrange
            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _vehiculoServiceMock
                .Setup(x => x.GetDetalleByIdAsync(1, 99))
                .ReturnsAsync(ServiceResult<VehiculoDetalle>.Fail(
                    ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                    "Vehículo no encontrado"));

            // Act
            var result = await _controller.GetById(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(notFoundResult.Value);
            Assert.Equal(ErrorCode.VEH_NO_ENCONTRADO.ToString(), error.Codigo);
        }

        [Fact]
        public async Task GetByMatricula_Encontrado_DebeRetornar200()
        {
            // Arrange
            var detalle = new VehiculoDetalle { Id = 3, Matricula = "2222CCC" };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _vehiculoServiceMock
                .Setup(x => x.GetDetalleByMatriculaAsync(1, "2222CCC"))
                .ReturnsAsync(ServiceResult<VehiculoDetalle>.Ok(detalle));

            // Act
            var result = await _controller.GetByMatricula("2222CCC");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Create_MatriculaDuplicada_DebeRetornar400()
        {
            // Arrange
            var request = new Vehiculo
            {
                TallerId = 1,
                Matricula = "3333DDD",
                MarcaId = 1,
                ModeloId = 1,
                VehiculoTipoId = 1
            };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _vehiculoServiceMock
                .Setup(x => x.RegistrarVehiculoAsync(1, request))
                .ReturnsAsync(ServiceResult<VehiculoDetalle>.Fail(
                    ErrorCode.VEH_MATRICULA_DUPLICADA.ToString(),
                    "Matrícula duplicada"));

            // Act
            var result = await _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
            Assert.Equal(ErrorCode.VEH_MATRICULA_DUPLICADA.ToString(), error.Codigo);
        }

        [Fact]
        public async Task Create_DatosValidos_DebeRetornar201()
        {
            // Arrange
            var request = new Vehiculo
            {
                TallerId = 1,
                Matricula = "4444EEE",
                MarcaId = 1,
                ModeloId = 1,
                VehiculoTipoId = 1
            };

            var detalle = new VehiculoDetalle { Id = 12, Matricula = "4444EEE" };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _vehiculoServiceMock
                .Setup(x => x.RegistrarVehiculoAsync(1, request))
                .ReturnsAsync(ServiceResult<VehiculoDetalle>.Ok(detalle));

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task Update_DatosValidos_DebeRetornar200()
        {
            // Arrange
            var request = new Vehiculo
            {
                TallerId = 1,
                Matricula = "5555FFF",
                MarcaId = 2,
                ModeloId = 3,
                VehiculoTipoId = 1
            };

            var detalle = new VehiculoDetalle { Id = 15, Matricula = "5555FFF" };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _vehiculoServiceMock
                .Setup(x => x.ActualizarVehiculoAsync(1, 15, request))
                .ReturnsAsync(ServiceResult<VehiculoDetalle>.Ok(detalle));

            // Act
            var result = await _controller.Update(15, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}
