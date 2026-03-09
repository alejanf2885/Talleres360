using Moq;
using Xunit;
using Talleres360.Services.Clientes;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Dtos.Clientes;
using Talleres360.Enums;

namespace Talleres360.Tests.Services.Clientes
{
    public class CustomerServiceTests
    {
        [Fact]
        public async Task CrearCliente_DebeRetornarError_CuandoPlanFreeLlegaAlLimite()
        {
            // 1. ARRANGE
            int tallerId = 5;
            Mock<ICustomerRepository> mockCustomerRepo = new Mock<ICustomerRepository>();
            Mock<ITallerRepository> mockTallerRepo = new Mock<ITallerRepository>();

            // Simulamos Taller en plan FREE (Id 1 según tu Enum) y ya NO es TRIAL
            Taller tallerPaco = new Taller
            {
                Id = tallerId,
                PlanId = (int)PlanTipo.FREE,
                TipoSuscripcion = "MENSUAL", // Ya no es prueba
                Activo = true
            };

            // Simulamos que el repositorio dice que YA TIENE 10 clientes
            mockCustomerRepo.Setup(r => r.CountByTallerIdAsync(tallerId)).ReturnsAsync(10);
            mockTallerRepo.Setup(r => r.GetByIdAsync(tallerId)).ReturnsAsync(tallerPaco);

            CustomerService service = new CustomerService(mockCustomerRepo.Object, mockTallerRepo.Object);

            CrearClienteRequest nuevaPeticion = new CrearClienteRequest
            {
                Nombre = "Cliente 11",
                Telefono = "000000"
            };

            // 2. ACT
            (bool Success, string Message, Cliente? Cliente) resultado =
                await service.CrearClienteAsync(tallerId, nuevaPeticion);

            // 3. ASSERT
            Assert.False(resultado.Success);
            Assert.Equal("Límite de 10 clientes alcanzado en el plan FREE.", resultado.Message);
            mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Never);
            // ^ Verificamos que NI SIQUIERA intentó guardarlo en la base de datos
        }
    }
}