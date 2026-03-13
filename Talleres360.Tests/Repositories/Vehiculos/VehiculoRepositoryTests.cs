using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Models;
using Talleres360.Repositories.Vehiculos;
using Xunit;

namespace Talleres360.Tests.Repositories.Vehiculos
{
    public class VehiculoRepositoryTests
    {
        [Fact]
        public async Task ExistsAsync_DebeRetornarTrue_CuandoMatriculaExiste()
        {
            using ApplicationDbContext context = BuildContext(nameof(ExistsAsync_DebeRetornarTrue_CuandoMatriculaExiste));
            context.Vehiculos.Add(new Vehiculo
            {
                Id = 1,
                TallerId = 1,
                Matricula = "EXI001",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1
            });
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);

            bool exists = await repository.ExistsAsync("EXI001");

            Assert.True(exists);
        }

        [Fact]
        public async Task GetAllDetalleByTallerAsync_DebeAplicarFiltroPaginacionYExcluirEliminados()
        {
            using ApplicationDbContext context = BuildContext(nameof(GetAllDetalleByTallerAsync_DebeAplicarFiltroPaginacionYExcluirEliminados));
            context.VehiculosDetalle.AddRange(
                new VehiculoDetalle
                {
                    Id = 1,
                    TallerId = 1,
                    Matricula = "ABC111",
                    MarcaId = 1,
                    ModeloId = 1,
                    TipoVehiculoId = 1,
                    Eliminado = false
                },
                new VehiculoDetalle
                {
                    Id = 2,
                    TallerId = 1,
                    Matricula = "ABC222",
                    MarcaId = 1,
                    ModeloId = 1,
                    TipoVehiculoId = 1,
                    Eliminado = false
                },
                new VehiculoDetalle
                {
                    Id = 3,
                    TallerId = 1,
                    Matricula = "ZZZ999",
                    MarcaId = 1,
                    ModeloId = 1,
                    TipoVehiculoId = 1,
                    Eliminado = true
                },
                new VehiculoDetalle
                {
                    Id = 4,
                    TallerId = 2,
                    Matricula = "ABC333",
                    MarcaId = 1,
                    ModeloId = 1,
                    TipoVehiculoId = 1,
                    Eliminado = false
                }
            );
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);
            var filtro = new VehiculoFiltroDto { Matricula = "ABC" };

            var result = await repository.GetAllDetalleByTallerAsync(1, 1, 10, filtro);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, x => Assert.Equal(1, x.TallerId));
            Assert.All(result.Data, x => Assert.Contains("ABC", x.Matricula));
        }

        [Fact]
        public async Task AddAsync_DebePersistirVehiculoEnContexto()
        {
            using ApplicationDbContext context = BuildContext(nameof(AddAsync_DebePersistirVehiculoEnContexto));
            var repository = new VehiculoRepository(context);
            var vehiculo = new Vehiculo
            {
                Id = 100,
                TallerId = 1,
                Matricula = "NEW100",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1
            };

            await repository.AddAsync(vehiculo);

            Vehiculo? saved = await context.Vehiculos.FirstOrDefaultAsync(v => v.Matricula == "NEW100");
            Assert.NotNull(saved);
            Assert.Equal(1, saved.TallerId);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarVehiculo_CuandoExiste()
        {
            using ApplicationDbContext context = BuildContext(nameof(GetByIdAsync_DebeRetornarVehiculo_CuandoExiste));
            context.Vehiculos.Add(new Vehiculo
            {
                Id = 10,
                TallerId = 1,
                Matricula = "GET010",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1
            });
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);
            Vehiculo? result = await repository.GetByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("GET010", result!.Matricula);
        }

        [Fact]
        public async Task GetByMatriculaAsync_DebeRetornarVehiculo_CuandoExiste()
        {
            using ApplicationDbContext context = BuildContext(nameof(GetByMatriculaAsync_DebeRetornarVehiculo_CuandoExiste));
            context.Vehiculos.Add(new Vehiculo
            {
                Id = 11,
                TallerId = 1,
                Matricula = "MAT011",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1
            });
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);
            Vehiculo? result = await repository.GetByMatriculaAsync("MAT011");

            Assert.NotNull(result);
            Assert.Equal(11, result!.Id);
        }

        [Fact]
        public async Task UpdateAsync_DebeActualizarVehiculo()
        {
            using ApplicationDbContext context = BuildContext(nameof(UpdateAsync_DebeActualizarVehiculo));
            context.Vehiculos.Add(new Vehiculo
            {
                Id = 12,
                TallerId = 1,
                Matricula = "UPD012",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1
            });
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);
            Vehiculo vehiculo = await context.Vehiculos.FirstAsync(v => v.Id == 12);
            vehiculo.Matricula = "UPD012X";

            await repository.UpdateAsync(vehiculo);

            Vehiculo? updated = await context.Vehiculos.FirstOrDefaultAsync(v => v.Id == 12);
            Assert.NotNull(updated);
            Assert.Equal("UPD012X", updated!.Matricula);
        }

        [Fact]
        public async Task GetDetalleByIdAsync_DebeRetornarDetalle_CuandoExiste()
        {
            using ApplicationDbContext context = BuildContext(nameof(GetDetalleByIdAsync_DebeRetornarDetalle_CuandoExiste));
            context.VehiculosDetalle.Add(new VehiculoDetalle
            {
                Id = 20,
                TallerId = 2,
                Matricula = "DET020",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1,
                Eliminado = false
            });
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);
            VehiculoDetalle? result = await repository.GetDetalleByIdAsync(20);

            Assert.NotNull(result);
            Assert.Equal("DET020", result!.Matricula);
        }

        [Fact]
        public async Task GetDetalleByMatriculaAsync_DebeRetornarDetalle_CuandoExiste()
        {
            using ApplicationDbContext context = BuildContext(nameof(GetDetalleByMatriculaAsync_DebeRetornarDetalle_CuandoExiste));
            context.VehiculosDetalle.Add(new VehiculoDetalle
            {
                Id = 21,
                TallerId = 3,
                Matricula = "DET021",
                MarcaId = 1,
                ModeloId = 1,
                TipoVehiculoId = 1,
                Eliminado = false
            });
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);
            VehiculoDetalle? result = await repository.GetDetalleByMatriculaAsync("DET021");

            Assert.NotNull(result);
            Assert.Equal(21, result!.Id);
        }

        [Fact]
        public async Task GetAllDetalleByTallerAsync_DebeFiltrarPorMarcaModeloTipoYAnio()
        {
            using ApplicationDbContext context = BuildContext(nameof(GetAllDetalleByTallerAsync_DebeFiltrarPorMarcaModeloTipoYAnio));
            context.VehiculosDetalle.AddRange(
                new VehiculoDetalle { Id = 30, TallerId = 1, Matricula = "FILT-1", MarcaId = 1, ModeloId = 10, TipoVehiculoId = 2, Anio = 2020, Eliminado = false },
                new VehiculoDetalle { Id = 31, TallerId = 1, Matricula = "FILT-2", MarcaId = 2, ModeloId = 20, TipoVehiculoId = 3, Anio = 2021, Eliminado = false },
                new VehiculoDetalle { Id = 32, TallerId = 1, Matricula = "FILT-3", MarcaId = 1, ModeloId = 10, TipoVehiculoId = 2, Anio = 2019, Eliminado = false }
            );
            await context.SaveChangesAsync();

            var repository = new VehiculoRepository(context);
            var filtro = new VehiculoFiltroDto
            {
                MarcaId = 1,
                ModeloId = 10,
                TipoVehiculoId = 2,
                Anio = 2020
            };

            var result = await repository.GetAllDetalleByTallerAsync(1, 1, 10, filtro);

            Assert.Equal(1, result.TotalCount);
            VehiculoDetalle single = Assert.Single(result.Data);
            Assert.Equal("FILT-1", single.Matricula);
        }

        private static ApplicationDbContext BuildContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
