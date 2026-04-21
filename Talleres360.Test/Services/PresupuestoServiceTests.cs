using Moq;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Services.Presupuestos;

namespace Talleres360.Test.Services
{
    public class PresupuestoServiceTests
    {
        private static PresupuestoService CrearService(
            Mock<ITrabajoRepository>? trabajoRepo = null,
            Mock<IVehiculoRepository>? vehiculoRepo = null)
        {
            return new PresupuestoService(
                (trabajoRepo ?? new Mock<ITrabajoRepository>()).Object,
                (vehiculoRepo ?? new Mock<IVehiculoRepository>()).Object);
        }

        [Fact]
        public async Task CrearAsync_Debe_Forzar_Estado_PRESUPUESTO_Independientemente_Del_Request()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Trabajo? trabajoCreado = null;

            trabajoRepoMock
                .Setup(r => r.GenerarNumeroDocumentoTrabajoAsync(1))
                .ReturnsAsync("TRAB-2026-000001");

            trabajoRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Trabajo>()))
                .Callback<Trabajo>(t => { t.Id = 1; trabajoCreado = t; })
                .Returns(Task.CompletedTask);

            trabajoRepoMock
                .Setup(r => r.ObtenerDetallePorIdAsync(1))
                .ReturnsAsync(new TrabajoDto { Id = 1, Estado = TrabajoEstado.PRESUPUESTO });

            PresupuestoService service = CrearService(trabajoRepoMock);

            CrearTrabajoRequest request = new CrearTrabajoRequest
            {
                KmEntrada = 0,
                Subtotal = 100,
                ImporteImpuestos = 21,
                Total = 121,
                DatosIncompletos = false
            };

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.CrearAsync(1, 5, request);

            Assert.True(resultado.Success);
            Assert.NotNull(trabajoCreado);
            Assert.Equal(TrabajoEstado.PRESUPUESTO, trabajoCreado!.Estado);
            Assert.Equal(TrabajoEstadoPago.PENDIENTE, trabajoCreado.EstadoPago);
        }

        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_Vehiculo_No_Pertenece_Al_Taller()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Mock<IVehiculoRepository> vehiculoRepoMock = new Mock<IVehiculoRepository>();
            vehiculoRepoMock
                .Setup(r => r.PerteneceATallerAsync(99, 1))
                .ReturnsAsync(false);

            PresupuestoService service = CrearService(trabajoRepoMock, vehiculoRepoMock);

            CrearTrabajoRequest request = new CrearTrabajoRequest
            {
                VehiculoId = 99,
                KmEntrada = 0,
                Subtotal = 0,
                ImporteImpuestos = 0,
                Total = 0,
                DatosIncompletos = false
            };

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.CrearAsync(1, 5, request);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.VEH_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
            trabajoRepoMock.Verify(r => r.AddAsync(It.IsAny<Trabajo>()), Times.Never);
        }

        [Fact]
        public async Task EnviarAsync_Debe_Cambiar_Estado_A_PRESUPUESTO_ENVIADO()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Trabajo trabajo = new Trabajo { Id = 1, TallerId = 1, Estado = TrabajoEstado.PRESUPUESTO };

            trabajoRepoMock.Setup(r => r.ObtenerEntidadPorIdAsync(1)).ReturnsAsync(trabajo);
            trabajoRepoMock.Setup(r => r.ObtenerDetallePorIdAsync(1))
                .ReturnsAsync(new TrabajoDto { Id = 1, Estado = TrabajoEstado.PRESUPUESTO_ENVIADO });

            PresupuestoService service = CrearService(trabajoRepoMock);

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.EnviarAsync(1, 1);

            Assert.True(resultado.Success);
            Assert.Equal(TrabajoEstado.PRESUPUESTO_ENVIADO, trabajo.Estado);
            Assert.NotNull(trabajo.FechaEnvioPresupuesto);
            Assert.NotNull(trabajo.ValidezHastaPresupuesto);
            trabajoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Trabajo>()), Times.Once);
        }

        [Fact]
        public async Task EnviarAsync_Debe_Fallar_Si_Estado_No_Es_PRESUPUESTO()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Trabajo trabajo = new Trabajo { Id = 1, TallerId = 1, Estado = TrabajoEstado.ABIERTO };

            trabajoRepoMock.Setup(r => r.ObtenerEntidadPorIdAsync(1)).ReturnsAsync(trabajo);

            PresupuestoService service = CrearService(trabajoRepoMock);

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.EnviarAsync(1, 1);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_TRANSICION_INVALIDA.ToString(), resultado.ErrorCode);
            trabajoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Trabajo>()), Times.Never);
        }

        [Fact]
        public async Task AceptarAsync_Debe_Cambiar_Estado_A_ABIERTO()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Trabajo trabajo = new Trabajo { Id = 1, TallerId = 1, Estado = TrabajoEstado.PRESUPUESTO_ENVIADO };

            trabajoRepoMock.Setup(r => r.ObtenerEntidadPorIdAsync(1)).ReturnsAsync(trabajo);
            trabajoRepoMock.Setup(r => r.ObtenerDetallePorIdAsync(1))
                .ReturnsAsync(new TrabajoDto { Id = 1, Estado = TrabajoEstado.ABIERTO });

            PresupuestoService service = CrearService(trabajoRepoMock);

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.AceptarAsync(1, 1, "https://firma.url");

            Assert.True(resultado.Success);
            Assert.Equal(TrabajoEstado.ABIERTO, trabajo.Estado);
            Assert.Equal("https://firma.url", trabajo.FirmaAceptacionUrl);
            Assert.NotNull(trabajo.FechaAceptacionPresupuesto);
            trabajoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Trabajo>()), Times.Once);
        }

        [Fact]
        public async Task RechazarAsync_Debe_Cambiar_Estado_A_RECHAZADO()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Trabajo trabajo = new Trabajo { Id = 1, TallerId = 1, Estado = TrabajoEstado.PRESUPUESTO_ENVIADO };

            trabajoRepoMock.Setup(r => r.ObtenerEntidadPorIdAsync(1)).ReturnsAsync(trabajo);
            trabajoRepoMock.Setup(r => r.ObtenerDetallePorIdAsync(1))
                .ReturnsAsync(new TrabajoDto { Id = 1, Estado = TrabajoEstado.RECHAZADO });

            PresupuestoService service = CrearService(trabajoRepoMock);

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.RechazarAsync(1, 1, "Precio demasiado alto");

            Assert.True(resultado.Success);
            Assert.Equal(TrabajoEstado.RECHAZADO, trabajo.Estado);
            Assert.Equal("Precio demasiado alto", trabajo.MotivoRechazo);
            Assert.NotNull(trabajo.FechaRechazo);
            trabajoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Trabajo>()), Times.Once);
        }

        [Fact]
        public async Task RechazarAsync_Debe_Fallar_Si_MotivoRechazo_Esta_Vacio()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Trabajo trabajo = new Trabajo { Id = 1, TallerId = 1, Estado = TrabajoEstado.PRESUPUESTO_ENVIADO };

            trabajoRepoMock.Setup(r => r.ObtenerEntidadPorIdAsync(1)).ReturnsAsync(trabajo);

            PresupuestoService service = CrearService(trabajoRepoMock);

            Dtos.Responses.ServiceResult<TrabajoDto> resultado = await service.RechazarAsync(1, 1, "   ");

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_DATOS_INVALIDOS.ToString(), resultado.ErrorCode);
            trabajoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Trabajo>()), Times.Never);
        }

        [Fact]
        public async Task EliminarAsync_Debe_Fallar_Si_No_Es_Presupuesto()
        {
            Mock<ITrabajoRepository> trabajoRepoMock = new Mock<ITrabajoRepository>();
            Trabajo trabajo = new Trabajo { Id = 1, TallerId = 1, Estado = TrabajoEstado.ABIERTO };

            trabajoRepoMock.Setup(r => r.ObtenerEntidadPorIdAsync(1)).ReturnsAsync(trabajo);

            PresupuestoService service = CrearService(trabajoRepoMock);

            Dtos.Responses.ServiceResult<bool> resultado = await service.EliminarAsync(1, 1);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_ESTADO_INVALIDO.ToString(), resultado.ErrorCode);
            trabajoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Trabajo>()), Times.Never);
        }
    }
}
