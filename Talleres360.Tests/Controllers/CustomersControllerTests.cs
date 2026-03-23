using Microsoft.AspNetCore.Mvc;
using Moq;
using Talleres360.Controllers;
using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;

namespace Talleres360.Tests.Controllers
{
    public class CustomersControllerTests
    {
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly Mock<IUserContextService> _userContextMock;
        private readonly CustomersController _controller;

        public CustomersControllerTests()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _userContextMock = new Mock<IUserContextService>();

            _controller = new CustomersController(
                _customerServiceMock.Object,
                _userContextMock.Object);
        }

        [Fact]
        public async Task GetAll_SinTallerIdEnJWT_DebeRetornar401()
        {
            // Arrange
            var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
            _userContextMock.Setup(x => x.GetTallerId()).Returns((int?)null);

            // Act
            var result = await _controller.GetAll(pagination, "ana");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            _customerServiceMock.Verify(
                x => x.ObtenerTodosPagedAsync(It.IsAny<int>(), It.IsAny<PaginationParams>(), It.IsAny<string?>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAll_ConTallerId_DebeRetornar200ConPaginacion()
        {
            // Arrange
            var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var response = new PagedResponse<Cliente>
            {
                Data = [new Cliente { Id = 1, Nombre = "Cliente 1", Telefono = "111", Email = "c1@test.com" }],
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _customerServiceMock
                .Setup(x => x.ObtenerTodosPagedAsync(1, pagination, "ana"))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAll(pagination, "ana");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _customerServiceMock.Verify(x => x.ObtenerTodosPagedAsync(1, pagination, "ana"), Times.Once);
        }

        [Fact]
        public async Task GetById_ClienteEncontrado_DebeRetornar200()
        {
            // Arrange
            var cliente = new Cliente
            {
                Id = 5,
                Nombre = "Cliente Test",
                Telefono = "222",
                Email = "cliente@test.com"
            };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _customerServiceMock
                .Setup(x => x.ObtenerPorIdAsync(1, 5))
                .ReturnsAsync(cliente);

            // Act
            var result = await _controller.GetById(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetById_ClienteNoEncontrado_DebeRetornar404()
        {
            // Arrange
            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _customerServiceMock
                .Setup(x => x.ObtenerPorIdAsync(1, 99))
                .ReturnsAsync((Cliente?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(notFoundResult.Value);
            Assert.Equal(ErrorCode.CUST_NO_ENCONTRADO.ToString(), error.Codigo);
        }

        [Fact]
        public async Task Create_DatosValidos_DebeRetornar201()
        {
            // Arrange
            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Nuevo",
                Telefono = "333",
                Email = "nuevo@test.com"
            };

            var cliente = new Cliente
            {
                Id = 10,
                Nombre = "Cliente Nuevo",
                Telefono = "333",
                Email = "nuevo@test.com"
            };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _customerServiceMock
                .Setup(x => x.CrearClienteAsync(1, request))
                .ReturnsAsync(ServiceResult<Cliente>.Ok(cliente));

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task Create_EmailDuplicado_DebeRetornar400()
        {
            // Arrange
            var request = new CrearClienteRequest
            {
                Nombre = "Cliente",
                Telefono = "444",
                Email = "duplicado@test.com"
            };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _customerServiceMock
                .Setup(x => x.CrearClienteAsync(1, request))
                .ReturnsAsync(ServiceResult<Cliente>.Fail(
                    ErrorCode.CUST_EMAIL_DUPLICADO.ToString(),
                    "Email duplicado"));

            // Act
            var result = await _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var error = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
            Assert.Equal(ErrorCode.CUST_EMAIL_DUPLICADO.ToString(), error.Codigo);
        }

        [Fact]
        public async Task Update_DatosValidos_DebeRetornar200()
        {
            // Arrange
            var request = new ActualizarClienteRequest
            {
                Nombre = "Cliente Editado",
                Telefono = "555",
                Email = "editado@test.com"
            };

            var cliente = new Cliente
            {
                Id = 7,
                Nombre = "Cliente Editado",
                Telefono = "555",
                Email = "editado@test.com"
            };

            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _customerServiceMock
                .Setup(x => x.ActualizarClienteAsync(1, 7, request))
                .ReturnsAsync(ServiceResult<Cliente>.Ok(cliente));

            // Act
            var result = await _controller.Update(7, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Delete_SoftDelete_DebeRetornar200()
        {
            // Arrange
            _userContextMock.Setup(x => x.GetTallerId()).Returns(1);
            _customerServiceMock
                .Setup(x => x.EliminarClienteAsync(1, 8))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            var result = await _controller.Delete(8);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}
