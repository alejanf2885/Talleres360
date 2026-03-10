using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using System.Reflection;
using System.Security.Claims;
using Talleres360.API.Controllers;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;
using Xunit;

namespace Talleres360.Tests.Controllers
{
    public class CustomersControllerTests
    {
        // ==========================================
        // TESTS PARA EL FILTRO RequiereSuscripcionActiva
        // ==========================================

        [Fact]
        public void RequiereSuscripcionActiva_DebeEstarAplicadoEnMetodosCorrectos()
        {
            // ARRANGE & ACT
            var controllerType = typeof(CustomersController);
            
            var createMethod = controllerType.GetMethod("Create");
            var updateMethod = controllerType.GetMethod("Update");
            var deleteMethod = controllerType.GetMethod("Delete");
            var getAllMethod = controllerType.GetMethod("GetAll");
            var getByIdMethod = controllerType.GetMethod("GetById");
            var searchMethod = controllerType.GetMethod("Search");

            // ASSERT - Verificar que los métodos de escritura tienen el atributo
            var createAttribute = createMethod?.GetCustomAttributes(typeof(RequiereSuscripcionActivaAttribute), false);
            var updateAttribute = updateMethod?.GetCustomAttributes(typeof(RequiereSuscripcionActivaAttribute), false);
            var deleteAttribute = deleteMethod?.GetCustomAttributes(typeof(RequiereSuscripcionActivaAttribute), false);
            var getAllAttribute = getAllMethod?.GetCustomAttributes(typeof(RequiereSuscripcionActivaAttribute), false);
            var getByIdAttribute = getByIdMethod?.GetCustomAttributes(typeof(RequiereSuscripcionActivaAttribute), false);
            var searchAttribute = searchMethod?.GetCustomAttributes(typeof(RequiereSuscripcionActivaAttribute), false);

            // Los métodos de escritura (Create, Update, Delete) DEBEN tener el atributo
            Assert.NotNull(createAttribute);
            Assert.Single(createAttribute);
            Assert.NotNull(updateAttribute);
            Assert.Single(updateAttribute);
            Assert.NotNull(deleteAttribute);
            Assert.Single(deleteAttribute);

            // Los métodos de lectura (GetAll, GetById, Search) NO deben tener el atributo
            Assert.Empty(getAllAttribute ?? Array.Empty<object>());
            Assert.Empty(getByIdAttribute ?? Array.Empty<object>());
            Assert.Empty(searchAttribute ?? Array.Empty<object>());
        }

        // ==========================================
        // TESTS PARA EL CONTROLADOR CustomersController
        // ==========================================

        [Fact]
        public async Task Create_DebeRetornar201_CuandoClienteSeCreaCorrectamente()
        {
            // ARRANGE
            int tallerId = 1;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Test",
                Telefono = "123456789"
            };

            var clienteCreado = new Cliente
            {
                Id = 10,
                TallerId = tallerId,
                Nombre = request.Nombre,
                Telefono = request.Telefono,
                Eliminado = false
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.CrearClienteAsync(tallerId, request))
                .ReturnsAsync((true, "Cliente registrado con éxito.", clienteCreado));

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.Create(request);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado);
            Assert.Equal("GetById", createdResult.ActionName);
            Assert.Equal(clienteCreado, createdResult.Value);
            mockCustomerService.Verify(x => x.CrearClienteAsync(tallerId, request), Times.Once);
        }

        [Fact]
        public async Task Create_DebeRetornar400_CuandoClienteNoSePuedeCrear()
        {
            // ARRANGE
            int tallerId = 1;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Test",
                Telefono = "123456789"
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.CrearClienteAsync(tallerId, request))
                .ReturnsAsync((false, "Límite de clientes alcanzado.", (Cliente?)null));

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.Create(request);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Create_DebeRetornar401_CuandoTallerIdEsNull()
        {
            // ARRANGE
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Test",
                Telefono = "123456789"
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns((int?)null);

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.Create(request);

            // ASSERT
            Assert.IsType<UnauthorizedResult>(resultado);
            mockCustomerService.Verify(x => x.CrearClienteAsync(It.IsAny<int>(), It.IsAny<CrearClienteRequest>()), Times.Never);
        }

        [Fact]
        public async Task Update_DebeRetornar200_CuandoClienteSeActualizaCorrectamente()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var request = new ActualizarClienteRequest
            {
                Nombre = "Cliente Actualizado",
                Telefono = "987654321"
            };

            var clienteActualizado = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = request.Nombre,
                Telefono = request.Telefono,
                Eliminado = false
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.ActualizarClienteAsync(tallerId, clienteId, request))
                .ReturnsAsync((true, "Cliente actualizado con éxito.", clienteActualizado));

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.Update(clienteId, request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.NotNull(okResult.Value);
            mockCustomerService.Verify(x => x.ActualizarClienteAsync(tallerId, clienteId, request), Times.Once);
        }

        [Fact]
        public async Task Update_DebeRetornar400_CuandoClienteNoSePuedeActualizar()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 999;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var request = new ActualizarClienteRequest
            {
                Nombre = "Cliente Test",
                Telefono = "123456789"
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.ActualizarClienteAsync(tallerId, clienteId, request))
                .ReturnsAsync((false, "Cliente no encontrado.", (Cliente?)null));

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.Update(clienteId, request);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Delete_DebeRetornar200_CuandoClienteSeEliminaCorrectamente()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.EliminarClienteAsync(tallerId, clienteId))
                .ReturnsAsync((true, "Cliente eliminado correctamente."));

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.Delete(clienteId);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.NotNull(okResult.Value);
            mockCustomerService.Verify(x => x.EliminarClienteAsync(tallerId, clienteId), Times.Once);
        }

        [Fact]
        public async Task Delete_DebeRetornar400_CuandoClienteNoSePuedeEliminar()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 999;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.EliminarClienteAsync(tallerId, clienteId))
                .ReturnsAsync((false, "Cliente no encontrado."));

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.Delete(clienteId);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task GetAll_DebeRetornar200_ConListaPaginadaDeClientes()
        {
            // ARRANGE
            int tallerId = 1;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var clientes = new List<Cliente>
            {
                new Cliente { Id = 1, TallerId = tallerId, Nombre = "Cliente 1", Eliminado = false },
                new Cliente { Id = 2, TallerId = tallerId, Nombre = "Cliente 2", Eliminado = false }
            };

            var pagedResponse = new Talleres360.Dtos.PagedResponse<Cliente>
            {
                Data = clientes,
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.ObtenerTodosPagedAsync(tallerId, It.IsAny<Talleres360.Dtos.PaginationParams>(), null))
                .ReturnsAsync(pagedResponse);

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.GetAll();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuestaPaginada = Assert.IsType<Talleres360.Dtos.PagedResponse<Cliente>>(okResult.Value);
            Assert.Equal(2, respuestaPaginada.Data.Count());
            Assert.Equal(1, respuestaPaginada.PageNumber);
            Assert.Equal(10, respuestaPaginada.PageSize);
            Assert.Equal(2, respuestaPaginada.TotalCount);
            Assert.Equal(1, respuestaPaginada.TotalPages);
            mockCustomerService.Verify(x => x.ObtenerTodosPagedAsync(tallerId, It.IsAny<Talleres360.Dtos.PaginationParams>(), null), Times.Once);
        }

        [Fact]
        public async Task GetAll_DebePasarParametrosDePaginacion()
        {
            // ARRANGE
            int tallerId = 1;
            int pageNumber = 2;
            int pageSize = 5;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var pagedResponse = new Talleres360.Dtos.PagedResponse<Cliente>
            {
                Data = new List<Cliente>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 10
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.ObtenerTodosPagedAsync(tallerId, It.Is<Talleres360.Dtos.PaginationParams>(p => p.PageNumber == pageNumber && p.PageSize == pageSize), null))
                .ReturnsAsync(pagedResponse);

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.GetAll(pageNumber, pageSize);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuestaPaginada = Assert.IsType<Talleres360.Dtos.PagedResponse<Cliente>>(okResult.Value);
            Assert.Equal(pageNumber, respuestaPaginada.PageNumber);
            Assert.Equal(pageSize, respuestaPaginada.PageSize);
            Assert.Equal(2, respuestaPaginada.TotalPages);
            mockCustomerService.Verify(x => x.ObtenerTodosPagedAsync(tallerId, It.Is<Talleres360.Dtos.PaginationParams>(p => p.PageNumber == pageNumber && p.PageSize == pageSize), null), Times.Once);
        }

        [Fact]
        public async Task GetById_DebeRetornar200_CuandoClienteExiste()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = "Cliente Test",
                Eliminado = false
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.ObtenerPorIdAsync(tallerId, clienteId))
                .ReturnsAsync(cliente);

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.GetById(clienteId);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(cliente, okResult.Value);
            mockCustomerService.Verify(x => x.ObtenerPorIdAsync(tallerId, clienteId), Times.Once);
        }

        [Fact]
        public async Task GetById_DebeRetornar404_CuandoClienteNoExiste()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 999;
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.ObtenerPorIdAsync(tallerId, clienteId))
                .ReturnsAsync((Cliente?)null);

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            // ACT
            var resultado = await controller.GetById(clienteId);

            // ASSERT
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);
            Assert.NotNull(notFoundResult.Value);
            mockCustomerService.Verify(x => x.ObtenerPorIdAsync(tallerId, clienteId), Times.Once);
        }

        [Fact]
        public async Task Search_DebeRetornar200_ConClientesFiltradosPaginados()
        {
            // ARRANGE
            int tallerId = 1;
            string busqueda = "Juan";
            var mockCustomerService = new Mock<ICustomerService>();
            var mockUserContext = new Mock<IUserContextService>();

            var clientes = new List<Cliente>
            {
                new Cliente { Id = 1, TallerId = tallerId, Nombre = "Juan Pérez", Eliminado = false }
            };

            var pagedResponse = new Talleres360.Dtos.PagedResponse<Cliente>
            {
                Data = clientes,
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };

            mockUserContext.Setup(x => x.GetTallerId()).Returns(tallerId);
            mockCustomerService.Setup(x => x.ObtenerTodosPagedAsync(tallerId, It.IsAny<Talleres360.Dtos.PaginationParams>(), busqueda))
                .ReturnsAsync(pagedResponse);

            var controller = new CustomersController(mockCustomerService.Object, mockUserContext.Object);

            var request = new BusquedaClienteRequest { Texto = busqueda };

            // ACT
            var resultado = await controller.Search(request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuestaPaginada = Assert.IsType<Talleres360.Dtos.PagedResponse<Cliente>>(okResult.Value);
            Assert.Single(respuestaPaginada.Data);
            Assert.Equal(1, respuestaPaginada.TotalCount);
            mockCustomerService.Verify(x => x.ObtenerTodosPagedAsync(tallerId, It.IsAny<Talleres360.Dtos.PaginationParams>(), busqueda), Times.Once);
        }
    }
}
