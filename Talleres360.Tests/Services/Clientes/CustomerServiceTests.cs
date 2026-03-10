using Moq;
using Xunit;
using Talleres360.Services.Clientes;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Dtos.Clientes;
using Talleres360.Enums;

namespace Talleres360.Tests.Services.Clientes
{
    public class CustomerServiceTests
    {
        // ==========================================
        // TESTS PARA CrearClienteAsync
        // ==========================================

        [Fact]
        public async Task CrearCliente_DebeRetornarError_CuandoTallerNoExiste()
        {
            // ARRANGE
            int tallerId = 999;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync((Taller?)null);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new CrearClienteRequest
            {
                Nombre = "Test",
                Telefono = "123456789"
            };

            // ACT
            var resultado = await service.CrearClienteAsync(tallerId, request);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Error: Taller no identificado.", resultado.Message);
            Assert.Null(resultado.Cliente);
            mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task CrearCliente_DebePermitirCrear_CuandoEsTrial()
        {
            // ARRANGE
            int tallerId = 1;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.FREE,
                TipoSuscripcion = "TRIAL",
                Activo = true
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockCustomerRepo.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Trial",
                Apellidos = "Test",
                Telefono = "123456789",
                Email = "test@test.com",
                AceptaComunicaciones = true
            };

            // ACT
            var resultado = await service.CrearClienteAsync(tallerId, request);

            // ASSERT
            Assert.True(resultado.Success);
            Assert.Equal("Cliente registrado con éxito.", resultado.Message);
            Assert.NotNull(resultado.Cliente);
            Assert.Equal(request.Nombre, resultado.Cliente.Nombre);
            Assert.Equal(request.Apellidos, resultado.Cliente.Apellidos);
            Assert.Equal(request.Telefono, resultado.Cliente.Telefono);
            Assert.Equal(request.Email, resultado.Cliente.Email);
            Assert.Equal(request.AceptaComunicaciones, resultado.Cliente.AceptaComunicaciones);
            Assert.False(resultado.Cliente.Eliminado);
            Assert.Equal(tallerId, resultado.Cliente.TallerId);
            mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Once);
            mockPlanRepo.Verify(r => r.GetPlanPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CrearCliente_DebeRetornarError_CuandoPlanFreeLlegaAlLimite()
        {
            // ARRANGE
            int tallerId = 5;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.FREE,
                TipoSuscripcion = "MENSUAL",
                Activo = true
            };

            var plan = new Plan
            {
                Id = (int)PlanTipo.FREE,
                Nombre = "FREE",
                LimiteClientes = 10
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(10);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.FREE)).ReturnsAsync(plan);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente 11",
                Telefono = "000000"
            };

            // ACT
            var resultado = await service.CrearClienteAsync(tallerId, request);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Contains("Límite de 10 clientes alcanzado", resultado.Message);
            Assert.Contains("plan FREE", resultado.Message);
            mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task CrearCliente_DebeRetornarError_CuandoPlanBasicoLlegaAlLimite()
        {
            // ARRANGE
            int tallerId = 6;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.BASICO,
                TipoSuscripcion = "MENSUAL",
                Activo = true
            };

            var plan = new Plan
            {
                Id = (int)PlanTipo.BASICO,
                Nombre = "BÁSICO",
                LimiteClientes = 20
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(20);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.BASICO)).ReturnsAsync(plan);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente 21",
                Telefono = "111111"
            };

            // ACT
            var resultado = await service.CrearClienteAsync(tallerId, request);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Contains("Límite de 20 clientes alcanzado", resultado.Message);
            Assert.Contains("plan BÁSICO", resultado.Message);
            mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task CrearCliente_DebePermitirCrear_CuandoPlanTieneLimiteIlimitado()
        {
            // ARRANGE
            int tallerId = 7;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.PRO,
                TipoSuscripcion = "MENSUAL",
                Activo = true
            };

            var plan = new Plan
            {
                Id = (int)PlanTipo.PRO,
                Nombre = "PRO",
                LimiteClientes = null // Ilimitado
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(100);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.PRO)).ReturnsAsync(plan);
            mockCustomerRepo.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Ilimitado",
                Telefono = "222222"
            };

            // ACT
            var resultado = await service.CrearClienteAsync(tallerId, request);

            // ASSERT
            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Cliente);
            mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public async Task CrearCliente_DebePermitirCrear_CuandoNoSeAlcanzaElLimite()
        {
            // ARRANGE
            int tallerId = 8;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.FREE,
                TipoSuscripcion = "MENSUAL",
                Activo = true
            };

            var plan = new Plan
            {
                Id = (int)PlanTipo.FREE,
                Nombre = "FREE",
                LimiteClientes = 10
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(5);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.FREE)).ReturnsAsync(plan);
            mockCustomerRepo.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Nuevo",
                Telefono = "333333"
            };

            // ACT
            var resultado = await service.CrearClienteAsync(tallerId, request);

            // ASSERT
            Assert.True(resultado.Success);
            Assert.Equal("Cliente registrado con éxito.", resultado.Message);
            Assert.NotNull(resultado.Cliente);
            mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public async Task CrearCliente_DebeUsarPlanFreePorDefecto_CuandoPlanIdEsNull()
        {
            // ARRANGE
            int tallerId = 9;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = null,
                TipoSuscripcion = "MENSUAL",
                Activo = true
            };

            var plan = new Plan
            {
                Id = (int)PlanTipo.FREE,
                Nombre = "FREE",
                LimiteClientes = 10
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(10);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.FREE)).ReturnsAsync(plan);
            mockCustomerRepo.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new CrearClienteRequest
            {
                Nombre = "Cliente Test",
                Telefono = "444444"
            };

            // ACT
            var resultado = await service.CrearClienteAsync(tallerId, request);

            // ASSERT
            mockPlanRepo.Verify(r => r.GetPlanPorIdAsync((int)PlanTipo.FREE), Times.Once);
        }

        // ==========================================
        // TESTS PARA ObtenerTodosAsync
        // ==========================================

        [Fact]
        public async Task ObtenerTodos_DebeRetornarListaDeClientes()
        {
            // ARRANGE
            int tallerId = 1;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var clientes = new List<Cliente>
            {
                new Cliente { Id = 1, TallerId = tallerId, Nombre = "Cliente 1", Eliminado = false },
                new Cliente { Id = 2, TallerId = tallerId, Nombre = "Cliente 2", Eliminado = false }
            };

            mockCustomerRepo.Setup(r => r.GetAllByTallerIdAsync(tallerId, null)).ReturnsAsync(clientes);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerTodosAsync(tallerId);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count());
            mockCustomerRepo.Verify(r => r.GetAllByTallerIdAsync(tallerId, null), Times.Once);
        }

        [Fact]
        public async Task ObtenerTodos_DebePasarParametroBusqueda()
        {
            // ARRANGE
            int tallerId = 1;
            string busqueda = "Juan";
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var clientes = new List<Cliente>
            {
                new Cliente { Id = 1, TallerId = tallerId, Nombre = "Juan", Eliminado = false }
            };

            mockCustomerRepo.Setup(r => r.GetAllByTallerIdAsync(tallerId, busqueda)).ReturnsAsync(clientes);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerTodosAsync(tallerId, busqueda);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Single(resultado);
            mockCustomerRepo.Verify(r => r.GetAllByTallerIdAsync(tallerId, busqueda), Times.Once);
        }

        // ==========================================
        // TESTS PARA ObtenerPorIdAsync
        // ==========================================

        [Fact]
        public async Task ObtenerPorId_DebeRetornarCliente_CuandoExisteYPerteneceAlTaller()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = "Cliente Test",
                Eliminado = false
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerPorIdAsync(tallerId, clienteId);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(clienteId, resultado.Id);
            Assert.Equal(tallerId, resultado.TallerId);
            Assert.Equal("Cliente Test", resultado.Nombre);
        }

        [Fact]
        public async Task ObtenerPorId_DebeRetornarNull_CuandoClienteNoExiste()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 999;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync((Cliente?)null);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerPorIdAsync(tallerId, clienteId);

            // ASSERT
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObtenerPorId_DebeRetornarNull_CuandoClientePerteneceAOtroTaller()
        {
            // ARRANGE
            int tallerId = 1;
            int otroTallerId = 2;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = otroTallerId,
                Nombre = "Cliente Otro Taller",
                Eliminado = false
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerPorIdAsync(tallerId, clienteId);

            // ASSERT
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObtenerPorId_DebeRetornarNull_CuandoClienteEstaEliminado()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = "Cliente Eliminado",
                Eliminado = true
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerPorIdAsync(tallerId, clienteId);

            // ASSERT
            Assert.Null(resultado);
        }

        // ==========================================
        // TESTS PARA ActualizarClienteAsync
        // ==========================================

        [Fact]
        public async Task ActualizarCliente_DebeActualizarCorrectamente_CuandoClienteExiste()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var clienteExistente = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = "Nombre Antiguo",
                Apellidos = "Apellido Antiguo",
                Telefono = "111111111",
                Email = "antiguo@test.com",
                AceptaComunicaciones = false,
                Eliminado = false
            };

            var request = new ActualizarClienteRequest
            {
                Nombre = "Nombre Nuevo",
                Apellidos = "Apellido Nuevo",
                Telefono = "222222222",
                Email = "nuevo@test.com",
                AceptaComunicaciones = true
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(clienteExistente);
            mockCustomerRepo.Setup(r => r.UpdateAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ActualizarClienteAsync(tallerId, clienteId, request);

            // ASSERT
            Assert.True(resultado.Success);
            Assert.Equal("Cliente actualizado con éxito.", resultado.Message);
            Assert.NotNull(resultado.Cliente);
            Assert.Equal(request.Nombre, resultado.Cliente.Nombre);
            Assert.Equal(request.Apellidos, resultado.Cliente.Apellidos);
            Assert.Equal(request.Telefono, resultado.Cliente.Telefono);
            Assert.Equal(request.Email, resultado.Cliente.Email);
            Assert.Equal(request.AceptaComunicaciones, resultado.Cliente.AceptaComunicaciones);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public async Task ActualizarCliente_DebeRetornarError_CuandoClienteNoExiste()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 999;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync((Cliente?)null);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new ActualizarClienteRequest
            {
                Nombre = "Test",
                Telefono = "123456789"
            };

            // ACT
            var resultado = await service.ActualizarClienteAsync(tallerId, clienteId, request);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Cliente no encontrado o no pertenece a su taller.", resultado.Message);
            Assert.Null(resultado.Cliente);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task ActualizarCliente_DebeRetornarError_CuandoClientePerteneceAOtroTaller()
        {
            // ARRANGE
            int tallerId = 1;
            int otroTallerId = 2;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = otroTallerId,
                Nombre = "Cliente Otro Taller",
                Eliminado = false
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new ActualizarClienteRequest
            {
                Nombre = "Test",
                Telefono = "123456789"
            };

            // ACT
            var resultado = await service.ActualizarClienteAsync(tallerId, clienteId, request);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Cliente no encontrado o no pertenece a su taller.", resultado.Message);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task ActualizarCliente_DebeRetornarError_CuandoClienteEstaEliminado()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = "Cliente Eliminado",
                Eliminado = true
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            var request = new ActualizarClienteRequest
            {
                Nombre = "Test",
                Telefono = "123456789"
            };

            // ACT
            var resultado = await service.ActualizarClienteAsync(tallerId, clienteId, request);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Cliente no encontrado o no pertenece a su taller.", resultado.Message);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Never);
        }

        // ==========================================
        // TESTS PARA EliminarClienteAsync
        // ==========================================

        [Fact]
        public async Task EliminarCliente_DebeEliminarCorrectamente_CuandoClienteExiste()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var clienteExistente = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = "Cliente a Eliminar",
                Eliminado = false
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(clienteExistente);
            mockCustomerRepo.Setup(r => r.UpdateAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.EliminarClienteAsync(tallerId, clienteId);

            // ASSERT
            Assert.True(resultado.Success);
            Assert.Equal("Cliente eliminado correctamente.", resultado.Message);
            Assert.True(clienteExistente.Eliminado);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.Is<Cliente>(c => c.Eliminado == true)), Times.Once);
        }

        [Fact]
        public async Task EliminarCliente_DebeRetornarError_CuandoClienteNoExiste()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 999;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync((Cliente?)null);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.EliminarClienteAsync(tallerId, clienteId);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Cliente no encontrado o no pertenece a su taller.", resultado.Message);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task EliminarCliente_DebeRetornarError_CuandoClientePerteneceAOtroTaller()
        {
            // ARRANGE
            int tallerId = 1;
            int otroTallerId = 2;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = otroTallerId,
                Nombre = "Cliente Otro Taller",
                Eliminado = false
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.EliminarClienteAsync(tallerId, clienteId);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Cliente no encontrado o no pertenece a su taller.", resultado.Message);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task EliminarCliente_DebeRetornarError_CuandoClienteYaEstaEliminado()
        {
            // ARRANGE
            int tallerId = 1;
            int clienteId = 10;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var cliente = new Cliente
            {
                Id = clienteId,
                TallerId = tallerId,
                Nombre = "Cliente Ya Eliminado",
                Eliminado = true
            };

            mockCustomerRepo.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(cliente);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.EliminarClienteAsync(tallerId, clienteId);

            // ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Cliente no encontrado o no pertenece a su taller.", resultado.Message);
            mockCustomerRepo.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Never);
        }

        // ==========================================
        // TESTS PARA ObtenerEstadisticasAsync
        // ==========================================

        [Fact]
        public async Task ObtenerEstadisticas_DebeRetornarEstadisticasCorrectas()
        {
            // ARRANGE
            int tallerId = 1;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.FREE,
                Activo = true
            };

            var plan = new Plan
            {
                Id = (int)PlanTipo.FREE,
                Nombre = "FREE",
                LimiteClientes = 10
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.FREE)).ReturnsAsync(plan);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(5);
            mockCustomerRepo.Setup(r => r.CountNuevosEsteMesAsync(tallerId)).ReturnsAsync(2);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerEstadisticasAsync(tallerId);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(5, resultado.TotalClientes);
            Assert.Equal(2, resultado.ClientesNuevosEsteMes);
            Assert.Equal(10, resultado.LimitePlan);
            Assert.Equal("FREE", resultado.NombrePlan);
        }

        [Fact]
        public async Task ObtenerEstadisticas_DebeLanzarExcepcion_CuandoTallerNoExiste()
        {
            // ARRANGE
            int tallerId = 999;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync((Taller?)null);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(async () => await service.ObtenerEstadisticasAsync(tallerId));
        }

        [Fact]
        public async Task ObtenerEstadisticas_DebeManejarPlanNull()
        {
            // ARRANGE
            int tallerId = 1;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = null,
                Activo = true
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.FREE)).ReturnsAsync((Plan?)null);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(3);
            mockCustomerRepo.Setup(r => r.CountNuevosEsteMesAsync(tallerId)).ReturnsAsync(1);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerEstadisticasAsync(tallerId);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.TotalClientes);
            Assert.Equal(1, resultado.ClientesNuevosEsteMes);
            Assert.Null(resultado.LimitePlan);
            Assert.Equal("Desconocido", resultado.NombrePlan);
        }

        [Fact]
        public async Task ObtenerEstadisticas_DebeManejarPlanConLimiteNull()
        {
            // ARRANGE
            int tallerId = 1;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();
            Mock<IPlanRepository> mockPlanRepo = new Mock<IPlanRepository>();

            var taller = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.PRO,
                Activo = true
            };

            var plan = new Plan
            {
                Id = (int)PlanTipo.PRO,
                Nombre = "PRO",
                LimiteClientes = null // Ilimitado
            };

            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(taller);
            mockPlanRepo.Setup(r => r.GetPlanPorIdAsync((int)PlanTipo.PRO)).ReturnsAsync(plan);
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(50);
            mockCustomerRepo.Setup(r => r.CountNuevosEsteMesAsync(tallerId)).ReturnsAsync(10);

            var service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object, mockPlanRepo.Object);

            // ACT
            var resultado = await service.ObtenerEstadisticasAsync(tallerId);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(50, resultado.TotalClientes);
            Assert.Equal(10, resultado.ClientesNuevosEsteMes);
            Assert.Null(resultado.LimitePlan);
            Assert.Equal("PRO", resultado.NombrePlan);
        }
    }
}
