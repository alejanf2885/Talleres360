using Moq;
using Talleres360.Dtos.Citas;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Citas;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Services.Citas;

namespace Talleres360.Test.Services
{
    public class CitaServiceTests
    {
        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Estado_Es_Nulo()
        {
            Mock<ICitaRepository> citaRepositoryMock = new Mock<ICitaRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            CitaService service = new CitaService(
                citaRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                vehiculoRepositoryMock.Object,
                unitOfWorkMock.Object);

            CrearCitaRequest request = new CrearCitaRequest
            {
                Estado = null,
                NombreClienteTemp = "Cliente temporal",
                FechaCita = DateTime.UtcNow
            };

            Dtos.Responses.ServiceResult<CitaDto> resultado = await service.CrearAsync(1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.CITA_ESTADO_INVALIDO.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_No_Hay_Vehiculo_Ni_NombreTemporal()
        {
            Mock<ICitaRepository> citaRepositoryMock = new Mock<ICitaRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            CitaService service = new CitaService(
                citaRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                vehiculoRepositoryMock.Object,
                unitOfWorkMock.Object);

            CrearCitaRequest request = new CrearCitaRequest
            {
                Estado = CitaEstado.PENDIENTE,
                VehiculoId = null,
                NombreClienteTemp = null,
                FechaCita = DateTime.UtcNow
            };

            Dtos.Responses.ServiceResult<CitaDto> resultado = await service.CrearAsync(1, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_DATOS_INVALIDOS.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task ConvertirATrabajoAsync_Debe_Fallar_Cuando_Cita_No_Tiene_Vehiculo()
        {
            Mock<ICitaRepository> citaRepositoryMock = new Mock<ICitaRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            citaRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(20))
                .ReturnsAsync(new Cita
                {
                    Id = 20,
                    TallerId = 1,
                    VehiculoId = null,
                    Estado = CitaEstado.PENDIENTE,
                    Eliminado = false
                });

            CitaService service = new CitaService(
                citaRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                vehiculoRepositoryMock.Object,
                unitOfWorkMock.Object);

            ConvertirCitaTrabajoRequest request = new ConvertirCitaTrabajoRequest
            {
                KmEntrada = 123
            };

            Dtos.Responses.ServiceResult<CitaTrabajoDto> resultado = await service.ConvertirATrabajoAsync(1, 20, 10, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_OPERACION_INVALIDA.ToString(), resultado.ErrorCode);
            trabajoRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trabajo>()), Times.Never);
        }

        [Fact]
        public async Task ConvertirATrabajoAsync_Debe_Completar_Flujo_Cuando_Datos_Son_Validos()
        {
            Mock<ICitaRepository> citaRepositoryMock = new Mock<ICitaRepository>();
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();

            Cita cita = new Cita
            {
                Id = 30,
                TallerId = 1,
                VehiculoId = 200,
                Estado = CitaEstado.PENDIENTE,
                Descripcion = "Diagnóstico inicial",
                Eliminado = false
            };

            citaRepositoryMock
                .Setup(repo => repo.ObtenerEntidadPorIdAsync(30))
                .ReturnsAsync(cita);

            vehiculoRepositoryMock
                .Setup(repo => repo.PerteneceATallerAsync(200, 1))
                .ReturnsAsync(true);

            trabajoRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Trabajo>()))
                .Callback<Trabajo>(trabajo => trabajo.Id = 999)
                .Returns(Task.CompletedTask);

            citaRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Cita>()))
                .Returns(Task.CompletedTask);

            unitOfWorkMock.Setup(item => item.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(item => item.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(item => item.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            CitaService service = new CitaService(
                citaRepositoryMock.Object,
                trabajoRepositoryMock.Object,
                vehiculoRepositoryMock.Object,
                unitOfWorkMock.Object);

            ConvertirCitaTrabajoRequest request = new ConvertirCitaTrabajoRequest
            {
                KmEntrada = 150,
                TituloMantenimiento = "Revisión completa"
            };

            Dtos.Responses.ServiceResult<CitaTrabajoDto> resultado = await service.ConvertirATrabajoAsync(1, 30, 7, request);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(30, resultado.Data!.CitaId);
            Assert.Equal(999, resultado.Data.TrabajoId);
            Assert.Equal(CitaEstado.COMPLETADA, cita.Estado);

            unitOfWorkMock.Verify(item => item.BeginTransactionAsync(), Times.Once);
            unitOfWorkMock.Verify(item => item.CommitTransactionAsync(), Times.Once);
            unitOfWorkMock.Verify(item => item.RollbackTransactionAsync(), Times.Never);
        }
    }
}
