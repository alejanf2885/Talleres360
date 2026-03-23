using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Models;
using Talleres360.Repositories.Vehiculos;

namespace Talleres360.Tests.Repositories
{
    public class VehiculoRepositoryTests
    {
        private static class DbContextFactory
        {
            public static ApplicationDbContext Create()
            {
                string databaseName = $"Talleres360Tests_{Guid.NewGuid():N}";
                string connectionString =
                    $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

                DbContextOptions<ApplicationDbContext> options =
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlServer(connectionString)
                        .Options;

                ApplicationDbContext context = new ApplicationDbContext(options);
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                return context;
            }
        }

        private ApplicationDbContext CreateContext() => DbContextFactory.Create();

        [Fact]
        public async Task ExistsAsync_MatriculaExisteNoEliminada_RetornaTrue()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new VehiculoRepository(context);

            var vehiculo = new Vehiculo
            {
                TallerId       = 1,
                ClienteId      = null,
                VehiculoTipoId = 1,
                MarcaId        = 1,
                ModeloId       = 1,
                Matricula      = "1234ABC",
                Eliminado      = false,
                FechaCreacion  = DateTime.UtcNow
            };
            context.Vehiculos.Add(vehiculo);
            await context.SaveChangesAsync();

            // Act
            bool resultado = await repo.ExistsAsync("1234ABC");

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_MatriculaExistePeroEliminada_RetornaFalse()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new VehiculoRepository(context);

            var vehiculo = new Vehiculo
            {
                TallerId       = 1,
                ClienteId      = null,
                VehiculoTipoId = 1,
                MarcaId        = 1,
                ModeloId       = 1,
                Matricula      = "1234ABC",
                Eliminado      = true,
                FechaCreacion  = DateTime.UtcNow
            };
            context.Vehiculos.Add(vehiculo);
            await context.SaveChangesAsync();

            // Act
            bool resultado = await repo.ExistsAsync("1234ABC");

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllDetalleByTallerAsync_FiltroPorMatricula()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new VehiculoRepository(context);

            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('VW_VehiculoDetalles', 'U') IS NULL
                BEGIN
                    CREATE TABLE [VW_VehiculoDetalles] (
                        [Id] INT NOT NULL,
                        [TallerId] INT NOT NULL,
                        [ClienteId] INT NULL,
                        [VehiculoTipoId] INT NOT NULL,
                        [MarcaId] INT NOT NULL,
                        [ModeloId] INT NOT NULL,
                        [Matricula] NVARCHAR(15) NOT NULL,
                        [Anno] INT NULL,
                        [KmActuales] INT NULL,
                        [PromedioKmDiarios] DECIMAL(18,2) NULL,
                        [FechaUltimaActualizacionKm] DATETIME2 NULL,
                        [Eliminado] BIT NOT NULL,
                        [MarcaNombre] NVARCHAR(100) NULL,
                        [ModeloNombre] NVARCHAR(100) NULL,
                        [TipoNombre] NVARCHAR(50) NULL,
                        [NotasPendientes] INT NOT NULL,
                        [TieneAviso] BIT NOT NULL
                    );
                END
            ");

            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO VW_VehiculoDetalles
                (Id, TallerId, ClienteId, VehiculoTipoId, MarcaId, ModeloId, Matricula, Eliminado, MarcaNombre, ModeloNombre, TipoNombre, NotasPendientes, TieneAviso)
                VALUES
                (1, 1, NULL, 1, 1, 1, '1111AAA', 0, 'Marca', 'Modelo', 'Tipo', 0, 0),
                (2, 1, NULL, 1, 1, 1, '1111BBB', 0, 'Marca', 'Modelo', 'Tipo', 0, 0),
                (3, 1, NULL, 1, 1, 1, '2222CCC', 0, 'Marca', 'Modelo', 'Tipo', 0, 0);
            ");

            var filtro = new VehiculoFiltroDto
            {
                Matricula = "1111"
            };

            // Act
            var resultado = await repo.GetAllDetalleByTallerAsync(1, 1, 10, filtro);

            // Assert
            resultado.Data.Count().Should().Be(2);
            resultado.Data.Any(x => x.Matricula == "2222CCC").Should().BeFalse();
        }

        [Fact]
        public async Task PerteneceATallerAsync_VehiculoDeOtroTaller_RetornaFalse()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new VehiculoRepository(context);

            var vehiculo = new Vehiculo
            {
                TallerId       = 2,
                ClienteId      = null,
                VehiculoTipoId = 1,
                MarcaId        = 1,
                ModeloId       = 1,
                Matricula      = "9999ZZZ",
                Eliminado      = false,
                FechaCreacion  = DateTime.UtcNow
            };
            context.Vehiculos.Add(vehiculo);
            await context.SaveChangesAsync();

            // Act
            bool resultado = await repo.PerteneceATallerAsync(vehiculo.Id, 1);

            // Assert
            resultado.Should().BeFalse();
        }
    }
}
