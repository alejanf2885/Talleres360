using Moq;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;
using Talleres360.Services.Vehiculos;

namespace Talleres360.Tests.Services.Vehiculos
{
    public class VehiculoServiceTests
    {
        [Fact]
        public async Task GetAllDetalleByTallerAsync_DebeDelegarEnRepositorio()
        {
            var repoMock = new Mock<IVehiculoRepository>();
            var expected = new PagedResponse<VehiculoDetalle>
            {
                Data = [new VehiculoDetalle { Id = 1, TallerId = 1, Matricula = "ABC123" }],
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };

            repoMock.Setup(x => x.GetAllDetalleByTallerAsync(1, 1, 10, null)).ReturnsAsync(expected);
            var service = new VehiculoService(repoMock.Object);

            var result = await service.GetAllDetalleByTallerAsync(1, 1, 10, null);

            Assert.Equal(1, result.TotalCount);
            repoMock.Verify(x => x.GetAllDetalleByTallerAsync(1, 1, 10, null), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_DebeRetornarValorDelRepositorio()
        {
            var repoMock = new Mock<IVehiculoRepository>();
            repoMock.Setup(x => x.ExistsAsync("ABC123")).ReturnsAsync(true);
            var service = new VehiculoService(repoMock.Object);

            bool exists = await service.ExistsAsync("ABC123");

            Assert.True(exists);
            repoMock.Verify(x => x.ExistsAsync("ABC123"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_DebeLlamarRepositorioUnaVez()
        {
            var repoMock = new Mock<IVehiculoRepository>();
            var service = new VehiculoService(repoMock.Object);
            var vehiculo = new Vehiculo
            {
                Id = 11,
                TallerId = 1,
                Matricula = "UPD001",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1
            };

            await service.UpdateAsync(vehiculo);

            repoMock.Verify(x => x.UpdateAsync(vehiculo), Times.Once);
        }

        [Fact]
        public async Task AddAsync_DebeDelegarEnRepositorio()
        {
            var repoMock = new Mock<IVehiculoRepository>();
            var service = new VehiculoService(repoMock.Object);
            var vehiculo = new Vehiculo
            {
                Id = 12,
                TallerId = 2,
                Matricula = "ADD001",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1
            };

            await service.AddAsync(vehiculo);

            repoMock.Verify(x => x.AddAsync(vehiculo), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarEntidadDelRepositorio()
        {
            var repoMock = new Mock<IVehiculoRepository>();
            repoMock.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new Vehiculo { Id = 5, Matricula = "ID005", TallerId = 1, MarcaId = 1, ModeloId = 1, TipoVehiculoId = 1 });
            var service = new VehiculoService(repoMock.Object);

            var result = await service.GetByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result!.Id);
        }

        [Fact]
        public async Task GetDetalleByMatriculaAsync_DebeDelegarEnRepositorio()
        {
            var repoMock = new Mock<IVehiculoRepository>();
            repoMock.Setup(x => x.GetDetalleByMatriculaAsync("DET001"))
                .ReturnsAsync(new VehiculoDetalle { Id = 1, Matricula = "DET001", TallerId = 1 });

            var service = new VehiculoService(repoMock.Object);
            var result = await service.GetDetalleByMatriculaAsync("DET001");

            Assert.NotNull(result);
            Assert.Equal("DET001", result!.Matricula);
            repoMock.Verify(x => x.GetDetalleByMatriculaAsync("DET001"), Times.Once);
        }
    }
}
