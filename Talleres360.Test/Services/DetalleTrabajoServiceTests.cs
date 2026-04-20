using Moq;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Services.Trabajos;

namespace Talleres360.Test.Services
{
    public class DetalleTrabajoServiceTests
    {
        private static (DetalleTrabajoService, Mock<IDetalleTrabajoRepository>, Mock<ITrabajoRepository>, Mock<ITarifaHoraRepository>) BuildService()
        {
            Mock<IDetalleTrabajoRepository> detalleRepo = new Mock<IDetalleTrabajoRepository>();
            Mock<ITrabajoRepository> trabajoRepo = new Mock<ITrabajoRepository>();
            Mock<ITarifaHoraRepository> tarifaRepo = new Mock<ITarifaHoraRepository>();
            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();

            DetalleTrabajoService service = new DetalleTrabajoService(
                detalleRepo.Object, trabajoRepo.Object, tarifaRepo.Object, uowMock.Object);

            return (service, detalleRepo, trabajoRepo, tarifaRepo);
        }

        [Fact]
        public async Task AgregarAsync_Debe_Fallar_Cuando_Trabajo_No_Existe()
        {
            (DetalleTrabajoService service, _, Mock<ITrabajoRepository> trabajoRepo, _) = BuildService();
            trabajoRepo.Setup(r => r.ObtenerEntidadPorIdAsync(1)).ReturnsAsync((Trabajo?)null);

            ServiceResult<DetalleTrabajoDto> resultado = await service.AgregarAsync(
                1, 1, new CrearDetalleTrabajoRequest { Concepto = "Aceite", Cantidad = 1, PrecioUnitario = 10 });

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task AgregarAsync_Debe_Fallar_Cuando_Trabajo_Pertenece_A_Otro_Taller()
        {
            (DetalleTrabajoService service, _, Mock<ITrabajoRepository> trabajoRepo, _) = BuildService();
            trabajoRepo.Setup(r => r.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(new Trabajo { Id = 1, TallerId = 99 });

            ServiceResult<DetalleTrabajoDto> resultado = await service.AgregarAsync(
                1, 1, new CrearDetalleTrabajoRequest { Concepto = "Aceite", Cantidad = 1, PrecioUnitario = 10 });

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.TRA_NO_ENCONTRADO.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task AgregarAsync_Debe_Usar_PrecioHora_De_Tarifa_Activa_Cuando_EsManoObra_Sin_Precio()
        {
            (DetalleTrabajoService service, Mock<IDetalleTrabajoRepository> detalleRepo,
                Mock<ITrabajoRepository> trabajoRepo, Mock<ITarifaHoraRepository> tarifaRepo) = BuildService();

            trabajoRepo.Setup(r => r.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(new Trabajo { Id = 1, TallerId = 1 });

            tarifaRepo.Setup(r => r.ObtenerActivaAsync(1))
                .ReturnsAsync(new TarifaHora { Id = 3, TallerId = 1, PrecioHora = 70m, Activa = true });

            DetalleTrabajo? detalleGuardado = null;
            detalleRepo.Setup(r => r.AddAsync(It.IsAny<DetalleTrabajo>()))
                .Callback<DetalleTrabajo>(d => detalleGuardado = d)
                .Returns(Task.CompletedTask);

            ServiceResult<DetalleTrabajoDto> resultado = await service.AgregarAsync(
                1, 1, new CrearDetalleTrabajoRequest
                {
                    Concepto = "Mano de obra",
                    EsManoObra = true,
                    HorasAplicadas = 2.5m
                });

            Assert.True(resultado.Success);
            Assert.NotNull(detalleGuardado);
            Assert.Equal(70m, detalleGuardado!.PrecioUnitario);
            Assert.Equal(70m, detalleGuardado.PrecioHoraAplicado);
            Assert.Equal(2.5m, detalleGuardado.Cantidad);
            Assert.Equal(3, detalleGuardado.TarifaHoraId);
        }

        [Fact]
        public async Task AgregarAsync_Debe_Usar_PrecioHoraAplicado_Del_Request_Cuando_Se_Proporciona()
        {
            (DetalleTrabajoService service, Mock<IDetalleTrabajoRepository> detalleRepo,
                Mock<ITrabajoRepository> trabajoRepo, Mock<ITarifaHoraRepository> tarifaRepo) = BuildService();

            trabajoRepo.Setup(r => r.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(new Trabajo { Id = 1, TallerId = 1 });

            tarifaRepo.Setup(r => r.ObtenerActivaAsync(1))
                .ReturnsAsync(new TarifaHora { Id = 3, TallerId = 1, PrecioHora = 70m, Activa = true });

            DetalleTrabajo? detalleGuardado = null;
            detalleRepo.Setup(r => r.AddAsync(It.IsAny<DetalleTrabajo>()))
                .Callback<DetalleTrabajo>(d => detalleGuardado = d)
                .Returns(Task.CompletedTask);

            ServiceResult<DetalleTrabajoDto> resultado = await service.AgregarAsync(
                1, 1, new CrearDetalleTrabajoRequest
                {
                    Concepto = "Mano obra especial",
                    EsManoObra = true,
                    HorasAplicadas = 1m,
                    PrecioHoraAplicado = 90m
                });

            Assert.True(resultado.Success);
            Assert.Equal(90m, detalleGuardado!.PrecioUnitario);
            Assert.Equal(90m, detalleGuardado.PrecioHoraAplicado);
            Assert.Equal(3, detalleGuardado.TarifaHoraId);
        }

        [Fact]
        public async Task AgregarAsync_No_Consulta_Tarifa_Cuando_No_Es_ManoObra()
        {
            (DetalleTrabajoService service, Mock<IDetalleTrabajoRepository> detalleRepo,
                Mock<ITrabajoRepository> trabajoRepo, Mock<ITarifaHoraRepository> tarifaRepo) = BuildService();

            trabajoRepo.Setup(r => r.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(new Trabajo { Id = 1, TallerId = 1 });

            detalleRepo.Setup(r => r.AddAsync(It.IsAny<DetalleTrabajo>())).Returns(Task.CompletedTask);

            await service.AgregarAsync(
                1, 1, new CrearDetalleTrabajoRequest
                {
                    Concepto = "Filtro", EsManoObra = false, Cantidad = 2, PrecioUnitario = 15m
                });

            tarifaRepo.Verify(r => r.ObtenerActivaAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task EliminarAsync_Debe_Fallar_Cuando_Detalle_No_Existe()
        {
            (DetalleTrabajoService service, Mock<IDetalleTrabajoRepository> detalleRepo,
                Mock<ITrabajoRepository> trabajoRepo, _) = BuildService();

            trabajoRepo.Setup(r => r.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(new Trabajo { Id = 1, TallerId = 1 });

            detalleRepo.Setup(r => r.GetByIdAsync(99, 1)).ReturnsAsync((DetalleTrabajo?)null);

            ServiceResult<bool> resultado = await service.EliminarAsync(1, 1, 99);

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task EliminarAsync_Debe_Marcar_Eliminado_True()
        {
            (DetalleTrabajoService service, Mock<IDetalleTrabajoRepository> detalleRepo,
                Mock<ITrabajoRepository> trabajoRepo, _) = BuildService();

            trabajoRepo.Setup(r => r.ObtenerEntidadPorIdAsync(1))
                .ReturnsAsync(new Trabajo { Id = 1, TallerId = 1 });

            DetalleTrabajo detalle = new DetalleTrabajo { Id = 5, TrabajoId = 1, Concepto = "Aceite", Eliminado = false };
            detalleRepo.Setup(r => r.GetByIdAsync(5, 1)).ReturnsAsync(detalle);
            detalleRepo.Setup(r => r.UpdateAsync(It.IsAny<DetalleTrabajo>())).Returns(Task.CompletedTask);

            ServiceResult<bool> resultado = await service.EliminarAsync(1, 1, 5);

            Assert.True(resultado.Success);
            Assert.True(detalle.Eliminado);
        }
    }
}
