using Moq;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Models;
using Talleres360.Services.Trabajos;

namespace Talleres360.Test.Services
{
    public class TarifaHoraServiceTests
    {
        [Fact]
        public async Task CrearAsync_Debe_Fallar_Cuando_PrecioHora_Es_Negativo()
        {
            Mock<ITarifaHoraRepository> repoMock = new Mock<ITarifaHoraRepository>();
            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            TarifaHoraService service = new TarifaHoraService(repoMock.Object, uowMock.Object);

            ServiceResult<TarifaHoraDto> resultado = await service.CrearAsync(
                1, 1, new CrearTarifaHoraRequest { PrecioHora = -1 });

            Assert.False(resultado.Success);
            Assert.Equal(ErrorCode.SYS_DATOS_INVALIDOS.ToString(), resultado.ErrorCode);
        }

        [Fact]
        public async Task CrearAsync_Debe_Crear_Tarifa_Con_FechaVigencia_Por_Defecto()
        {
            Mock<ITarifaHoraRepository> repoMock = new Mock<ITarifaHoraRepository>();
            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();

            TarifaHora? tarifaGuardada = null;
            repoMock.Setup(r => r.AddAsync(It.IsAny<TarifaHora>()))
                .Callback<TarifaHora>(t => tarifaGuardada = t)
                .Returns(Task.CompletedTask);

            TarifaHoraService service = new TarifaHoraService(repoMock.Object, uowMock.Object);

            ServiceResult<TarifaHoraDto> resultado = await service.CrearAsync(
                1, 99, new CrearTarifaHoraRequest { PrecioHora = 45m });

            Assert.True(resultado.Success);
            Assert.NotNull(tarifaGuardada);
            Assert.Equal(45m, tarifaGuardada!.PrecioHora);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), tarifaGuardada.FechaVigencia);
            Assert.True(tarifaGuardada.Activa);
            Assert.Equal(99, tarifaGuardada.CreadoPorId);
        }

        [Fact]
        public async Task CrearAsync_Debe_Usar_FechaVigencia_Del_Request_Si_Se_Proporciona()
        {
            Mock<ITarifaHoraRepository> repoMock = new Mock<ITarifaHoraRepository>();
            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();

            DateOnly fechaEsperada = new DateOnly(2026, 6, 1);
            TarifaHora? tarifaGuardada = null;
            repoMock.Setup(r => r.AddAsync(It.IsAny<TarifaHora>()))
                .Callback<TarifaHora>(t => tarifaGuardada = t)
                .Returns(Task.CompletedTask);

            TarifaHoraService service = new TarifaHoraService(repoMock.Object, uowMock.Object);

            await service.CrearAsync(1, 1, new CrearTarifaHoraRequest
            {
                PrecioHora = 60m,
                FechaVigencia = fechaEsperada
            });

            Assert.Equal(fechaEsperada, tarifaGuardada!.FechaVigencia);
        }

        [Fact]
        public async Task CrearAsync_Debe_Normalizar_Descripcion_Nula_Cuando_Esta_Vacia()
        {
            Mock<ITarifaHoraRepository> repoMock = new Mock<ITarifaHoraRepository>();
            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();

            TarifaHora? tarifaGuardada = null;
            repoMock.Setup(r => r.AddAsync(It.IsAny<TarifaHora>()))
                .Callback<TarifaHora>(t => tarifaGuardada = t)
                .Returns(Task.CompletedTask);

            TarifaHoraService service = new TarifaHoraService(repoMock.Object, uowMock.Object);

            await service.CrearAsync(1, 1, new CrearTarifaHoraRequest { PrecioHora = 30m, Descripcion = "   " });

            Assert.Null(tarifaGuardada!.Descripcion);
        }

        [Fact]
        public async Task ObtenerActivaAsync_Debe_Retornar_Null_Data_Cuando_No_Hay_Tarifa_Activa()
        {
            Mock<ITarifaHoraRepository> repoMock = new Mock<ITarifaHoraRepository>();
            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            repoMock.Setup(r => r.ObtenerActivaAsync(1)).ReturnsAsync((TarifaHora?)null);

            TarifaHoraService service = new TarifaHoraService(repoMock.Object, uowMock.Object);

            ServiceResult<TarifaHoraDto?> resultado = await service.ObtenerActivaAsync(1);

            Assert.True(resultado.Success);
            Assert.Null(resultado.Data);
        }

        [Fact]
        public async Task ObtenerActivaAsync_Debe_Retornar_Dto_Cuando_Hay_Tarifa_Activa()
        {
            Mock<ITarifaHoraRepository> repoMock = new Mock<ITarifaHoraRepository>();
            Mock<IUnitOfWork> uowMock = new Mock<IUnitOfWork>();
            repoMock.Setup(r => r.ObtenerActivaAsync(1)).ReturnsAsync(new TarifaHora
            {
                Id = 5, TallerId = 1, PrecioHora = 55m, Activa = true,
                FechaVigencia = new DateOnly(2026, 1, 1), FechaCreacion = DateTime.UtcNow
            });

            TarifaHoraService service = new TarifaHoraService(repoMock.Object, uowMock.Object);

            ServiceResult<TarifaHoraDto?> resultado = await service.ObtenerActivaAsync(1);

            Assert.True(resultado.Success);
            Assert.NotNull(resultado.Data);
            Assert.Equal(55m, resultado.Data!.PrecioHora);
            Assert.True(resultado.Data.Activa);
        }
    }
}
