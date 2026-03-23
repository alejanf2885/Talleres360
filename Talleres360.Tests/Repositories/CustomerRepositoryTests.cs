using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Models;
using Talleres360.Repositories.Clientes;

namespace Talleres360.Tests.Repositories
{
    public class CustomerRepositoryTests
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
        public async Task GetAllByTallerIdPagedAsync_PaginacionCorrecta()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new CustomerRepository(context);

            DateTime now = DateTime.UtcNow;
            for (int i = 1; i <= 10; i++)
            {
                var cliente = new Cliente
                {
                    TallerId      = 1,
                    Nombre        = $"Cliente {i}",
                    Telefono      = $"60000000{i}",
                    Email         = $"cliente{i}@test.com",
                    Eliminado     = false,
                    FechaCreacion = now.AddMinutes(i)
                };
                context.Clientes.Add(cliente);
            }
            await context.SaveChangesAsync();

            var pagination = new PaginationParams
            {
                PageNumber = 2,
                PageSize   = 10
            };

            typeof(PaginationParams)
                .GetField("_pageSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(pagination, 3);

            // Act
            PagedResponse<Cliente> resultado = await repo.GetAllByTallerIdPagedAsync(1, pagination);

            // Assert
            resultado.Data.Count().Should().Be(3);
            resultado.TotalPages.Should().Be(4);
            resultado.HasPreviousPage.Should().BeTrue();
            resultado.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllByTallerIdPagedAsync_BusquedaPorNombre()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new CustomerRepository(context);

            context.Clientes.AddRange(
                new Cliente
                {
                    TallerId      = 1,
                    Nombre        = "María García",
                    Telefono      = "600000001",
                    Email         = "maria1@test.com",
                    Eliminado     = false,
                    FechaCreacion = DateTime.UtcNow
                },
                new Cliente
                {
                    TallerId      = 1,
                    Nombre        = "Carlos López",
                    Telefono      = "600000002",
                    Email         = "carlos@test.com",
                    Eliminado     = false,
                    FechaCreacion = DateTime.UtcNow
                },
                new Cliente
                {
                    TallerId      = 1,
                    Nombre        = "María Pérez",
                    Telefono      = "600000003",
                    Email         = "maria2@test.com",
                    Eliminado     = false,
                    FechaCreacion = DateTime.UtcNow
                });
            await context.SaveChangesAsync();

            var pagination = new PaginationParams
            {
                PageNumber = 1,
                PageSize   = 10
            };

            // Act
            PagedResponse<Cliente> resultado = await repo.GetAllByTallerIdPagedAsync(1, pagination, "María");

            // Assert
            resultado.Data.Count().Should().Be(2);
            resultado.Data.Any(x => x.Nombre == "Carlos López").Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByEmailAsync_EmailExisteEnMismoTaller_RetornaTrue()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new CustomerRepository(context);

            var cliente = new Cliente
            {
                TallerId      = 1,
                Nombre        = "Ana",
                Telefono      = "600000001",
                Email         = "ana@test.com",
                Eliminado     = false,
                FechaCreacion = DateTime.UtcNow
            };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Act
            bool resultado = await repo.ExistsByEmailAsync(1, "ana@test.com");

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByEmailAsync_EmailExisteEnOtroTaller_RetornaFalse()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new CustomerRepository(context);

            var cliente = new Cliente
            {
                TallerId      = 2,
                Nombre        = "Ana",
                Telefono      = "600000001",
                Email         = "ana@test.com",
                Eliminado     = false,
                FechaCreacion = DateTime.UtcNow
            };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Act
            bool resultado = await repo.ExistsByEmailAsync(1, "ana@test.com");

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByNifAsync_NifDuplicadoMismoTaller_RetornaTrue()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new CustomerRepository(context);

            var cliente = new Cliente
            {
                TallerId      = 1,
                Nombre        = "Ana",
                Telefono      = "600000001",
                Email         = "ana@test.com",
                NifCif        = "12345678A",
                Eliminado     = false,
                FechaCreacion = DateTime.UtcNow
            };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Act
            bool resultado = await repo.ExistsByNifAsync(1, "12345678A");

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task PerteneceATallerAsync_RecursoDeOtroTaller_RetornaFalse()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new CustomerRepository(context);

            var cliente = new Cliente
            {
                TallerId      = 2,
                Nombre        = "Ana",
                Telefono      = "600000001",
                Email         = "ana@test.com",
                Eliminado     = false,
                FechaCreacion = DateTime.UtcNow
            };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Act
            bool resultado = await repo.PerteneceATallerAsync(cliente.Id, 1);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task CountNuevosEsteMesAsync_SoloCuentaDelMesActual()
        {
            // Arrange
            ApplicationDbContext context = CreateContext();
            var repo = new CustomerRepository(context);

            context.Clientes.AddRange(
                new Cliente
                {
                    TallerId      = 1,
                    Nombre        = "Ana",
                    Telefono      = "600000001",
                    Email         = "ana1@test.com",
                    Eliminado     = false,
                    FechaCreacion = DateTime.UtcNow
                },
                new Cliente
                {
                    TallerId      = 1,
                    Nombre        = "Luis",
                    Telefono      = "600000002",
                    Email         = "luis@test.com",
                    Eliminado     = false,
                    FechaCreacion = DateTime.UtcNow
                },
                new Cliente
                {
                    TallerId      = 1,
                    Nombre        = "Marta",
                    Telefono      = "600000003",
                    Email         = "marta@test.com",
                    Eliminado     = false,
                    FechaCreacion = DateTime.UtcNow.AddMonths(-1)
                });
            await context.SaveChangesAsync();

            // Act
            int resultado = await repo.CountNuevosEsteMesAsync(1);

            // Assert
            resultado.Should().Be(2);
        }
    }
}
