using Moq;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Facturacion;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Services.Trabajos;

namespace Talleres360.Test.Services
{
    public class TrabajoServiceTests
    {
        private static TrabajoService CrearService(
            Mock<ITrabajoRepository>? trabajoRepo = null,
            Mock<IVehiculoRepository>? vehiculoRepo = null,
            Mock<IFacturacionService>? facturacionService = null)
        {
            return new TrabajoService(
                (trabajoRepo ?? new Mock<ITrabajoRepository>()).Object,
                (vehiculoRepo ?? new Mock<IVehiculoRepository>()).Object,
                (facturacionService ?? new Mock<IFacturacionService>()).Object);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Estado_Es_Nulo()
        {
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();
            TrabajoService service = CrearService(trabajoRepositoryMock, vehiculoRepositoryMock);

            CrearTrabajoRequest request = new CrearTrabajoRequest
            {
                Estado = null,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                KmEntrada = 100,
                Subtotal = 0,
                ImporteImpuestos = 0,
                Total = 0,
                DatosIncompletos = true
            };

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.CrearAsync(1, 10, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_ESTADO_INVALIDO.ToString(), resultado.ErrorCode);
            trabajoRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trabajo>()), Times.Never);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_EstadoPago_Es_Nulo()
        {
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();
            TrabajoService service = CrearService(trabajoRepositoryMock, vehiculoRepositoryMock);

            CrearTrabajoRequest request = new CrearTrabajoRequest
            {
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = null,
                KmEntrada = 100,
                Subtotal = 0,
                ImporteImpuestos = 0,
                Total = 0,
                DatosIncompletos = true
            };

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.CrearAsync(1, 10, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_ESTADO_PAGO_INVALIDO.ToString(), resultado.ErrorCode);
            trabajoRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trabajo>()), Times.Never);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Vehiculo_No_Pertenece_Al_Taller()
        {
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();
            vehiculoRepositoryMock
                .Setup(repo => repo.PerteneceATallerAsync(15, 1))
                .ReturnsAsync(false);

            TrabajoService service = CrearService(trabajoRepositoryMock, vehiculoRepositoryMock);

            CrearTrabajoRequest request = new CrearTrabajoRequest
            {
                VehiculoId = 15,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                KmEntrada = 100,
                Subtotal = 0,
                ImporteImpuestos = 0,
                Total = 0,
                DatosIncompletos = false
            };

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.CrearAsync(1, 10, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.VEH_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
            trabajoRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trabajo>()), Times.Never);
        }

        [Fact]
        public async Task CrearAsync_Debe_Crear_Trabajo_Con_DatosIncompletos_Cuando_No_Hay_Vehiculo()
        {
            Mock<ITrabajoRepository> trabajoRepositoryMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepositoryMock = new Mock<IVehiculoRepository>();

            Trabajo? trabajoCreado = null;

            trabajoRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Trabajo>()))
                .Callback<Trabajo>(trabajo =>
                {
                    trabajo.Id = 50;
                    trabajoCreado = trabajo;
                })
                .Returns(Task.CompletedTask);

            trabajoRepositoryMock
                .Setup(repo => repo.ObtenerDetallePorIdAsync(50))
                .ReturnsAsync(new TrabajoDto
                {
                    Id = 50,
                    Estado = TrabajoEstado.ABIERTO,
                    EstadoPago = TrabajoEstadoPago.PENDIENTE,
                    DatosIncompletos = true
                });

            TrabajoService service = CrearService(trabajoRepositoryMock, vehiculoRepositoryMock);

            CrearTrabajoRequest request = new CrearTrabajoRequest
            {
                VehiculoId = null,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                KmEntrada = 100,
                Subtotal = 0,
                ImporteImpuestos = 0,
                Total = 0,
                DatosIncompletos = false
            };

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.CrearAsync(1, 10, request);

            Assert.True(resultado.Success);
            Assert.NotNull(trabajoCreado);
            Assert.True(trabajoCreado!.DatosIncompletos);
            Assert.Null(trabajoCreado.VehiculoId);
        }
    }
}
