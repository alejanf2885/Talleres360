using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Talleres360.Dtos.Clientes;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Planes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Services.Clientes;
using Talleres360.Enums;

namespace Talleres360.Tests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<ITallerRepository> _tallerRepoMock;
        private readonly Mock<IPlanRepository> _planRepoMock;
        private readonly CustomerService _sut;

        public CustomerServiceTests()
        {
            _customerRepoMock = new Mock<ICustomerRepository>();
            _tallerRepoMock = new Mock<ITallerRepository>();
            _planRepoMock = new Mock<IPlanRepository>();

            _sut = new CustomerService(
                _customerRepoMock.Object,
                _tallerRepoMock.Object,
                _planRepoMock.Object
            );
        }

        [Fact]
        public async Task CrearClienteAsync_TallerNoEncontrado_DebeRetornarFail()
        {
            // Arrange
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Taller)null!);

            // Act
            var result = await _sut.CrearClienteAsync(1, new CrearClienteRequest { Email = "test@test.com", Telefono = "123" });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task CrearClienteAsync_LimitePlanAlcanzado_DebeRetornarFail()
        {
            // Arrange
            var taller = new Taller { Id = 1, TipoSuscripcion = "FREE", PlanId = (int?)PlanTipo.FREE };
            var plan = new Plan { Id = 1, LimiteClientes = 10, Nombre = "FREE plan" };
            
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);
            _customerRepoMock.Setup(x => x.CountByTallerIdAsync(1)).ReturnsAsync(10);
            _planRepoMock.Setup(x => x.GetPlanPorIdAsync((int)PlanTipo.FREE)).ReturnsAsync(plan);

            // Act
            var result = await _sut.CrearClienteAsync(1, new CrearClienteRequest { Email = "test@test.com", Telefono = "123" });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.CUST_LIMITE_PLAN_ALCANZADO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task CrearClienteAsync_TrialNoCompruebaLimite()
        {
            // Arrange
            var taller = new Taller { Id = 1, TipoSuscripcion = "TRIAL", PlanId = (int?)PlanTipo.FREE };
            
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);
            _customerRepoMock.Setup(x => x.ExistsByEmailAsync(1, "test@test.com")).ReturnsAsync(false);
            _customerRepoMock
                .Setup(x => x.AddAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.CrearClienteAsync(1, new CrearClienteRequest { Email = "test@test.com", Nombre="Paco", Telefono="123" });

            // Assert
            Assert.True(result.Success);
            _customerRepoMock.Verify(x => x.CountByTallerIdAsync(It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public async Task CrearClienteAsync_EmailDuplicado_DebeRetornarFail()
        {
            // Arrange
            var taller = new Taller { Id = 1, TipoSuscripcion = "TRIAL", PlanId = 1 };
            
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);
            _customerRepoMock.Setup(x => x.ExistsByEmailAsync(1, "dup@test.com")).ReturnsAsync(true);
            
            // Act
            var result = await _sut.CrearClienteAsync(1, new CrearClienteRequest { Email = "dup@test.com", Telefono="123" });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.CUST_EMAIL_DUPLICADO.ToString(), result.ErrorCode);
        }
        
        [Fact]
        public async Task CrearClienteAsync_NifDuplicado_DebeRetornarFail()
        {
            // Arrange
            var taller = new Taller { Id = 1, TipoSuscripcion = "TRIAL", PlanId = 1 };
            
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);
            _customerRepoMock.Setup(x => x.ExistsByEmailAsync(1, "ok@test.com")).ReturnsAsync(false);
            _customerRepoMock.Setup(x => x.ExistsByNifAsync(1, "12345678A")).ReturnsAsync(true);
            
            // Act
            var result = await _sut.CrearClienteAsync(1, new CrearClienteRequest { Email = "ok@test.com", NifCif=" 12345678 A ", Telefono="123" });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.CUST_DNI_DUPLICADO.ToString(), result.ErrorCode);
        }
        
        [Fact]
        public async Task CrearClienteAsync_DatosCorrectos_DebeRetornarOk()
        {
            // Arrange
            var taller = new Taller { Id = 1, TipoSuscripcion = "TRIAL", PlanId = 1 };
            
            _tallerRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(taller);
            _customerRepoMock.Setup(x => x.ExistsByEmailAsync(1, "ok@test.com")).ReturnsAsync(false);
            _customerRepoMock.Setup(x => x.ExistsByNifAsync(1, "12345678A")).ReturnsAsync(false);
            
            // Act
            var result = await _sut.CrearClienteAsync(1, new CrearClienteRequest { 
                Email = "ok@test.com", NifCif="12345678A", Nombre="Paco", Telefono="123" 
            });

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("12345678A", result.Data.NifCif);
            _customerRepoMock.Verify(x => x.AddAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public async Task ActualizarClienteAsync_ClienteNoPerteneceAlTaller_DebeRetornarFail()
        {
            // Arrange
            var clienteExistente = new Cliente { Id = 10, TallerId = 2 }; // Taller distinto
            _customerRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(clienteExistente);
            
            // Act
            var result = await _sut.ActualizarClienteAsync(1, 10, new ActualizarClienteRequest { Email = "x@test.com" });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.CUST_NO_ENCONTRADO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task ActualizarClienteAsync_EmailCambiadoYDuplicado_DebeRetornarFail()
        {
            // Arrange
            var clienteExistente = new Cliente { Id = 10, TallerId = 1, Email = "viejo@test.com" };
            _customerRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(clienteExistente);
            
            string emailNuevo = "enuso@test.com";
            _customerRepoMock.Setup(x => x.ExistsByEmailAsync(1, emailNuevo)).ReturnsAsync(true);
            
            // Act
            var result = await _sut.ActualizarClienteAsync(1, 10, new ActualizarClienteRequest { Email = emailNuevo });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ErrorCode.CUST_EMAIL_DUPLICADO.ToString(), result.ErrorCode);
        }

        [Fact]
        public async Task EliminarClienteAsync_SoftDeleteCorrecto()
        {
            // Arrange
            var clienteExistente = new Cliente { Id = 10, TallerId = 1, Eliminado = false };
            _customerRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(clienteExistente);
            
            // Act
            var result = await _sut.EliminarClienteAsync(1, 10);

            // Assert
            Assert.True(result.Success);
            _customerRepoMock.Verify(x => x.UpdateAsync(It.Is<Cliente>(c => c.Eliminado == true)), Times.Once);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_ClienteDeOtroTaller_DebeRetornarNull()
        {
            // Arrange
            var clienteExistente = new Cliente { Id = 10, TallerId = 2 };
            _customerRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(clienteExistente);
            
            // Act
            var result = await _sut.ObtenerPorIdAsync(1, 10);

            // Assert
            Assert.Null(result);
        }
    }
}