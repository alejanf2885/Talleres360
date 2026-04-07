using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Enums;
using Talleres360.Repositories.Trabajos;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class TrabajoRepositoryTests
    {
        [Fact]
        public async Task ObtenerTodosPagedAsync_Debe_Filtrar_Por_Estado_Y_DatosIncompletos()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Trabajo trabajoValido = new Trabajo
            {
                Id = 1,
                TallerId = 1,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                DatosIncompletos = true,
                KmEntrada = 100,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            Trabajo trabajoOtroEstado = new Trabajo
            {
                Id = 2,
                TallerId = 1,
                Estado = TrabajoEstado.CERRADO,
                EstadoPago = TrabajoEstadoPago.PAGADO,
                DatosIncompletos = true,
                KmEntrada = 100,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            Trabajo trabajoCompleto = new Trabajo
            {
                Id = 3,
                TallerId = 1,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                DatosIncompletos = false,
                KmEntrada = 100,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            await context.Trabajos.AddRangeAsync(trabajoValido, trabajoOtroEstado, trabajoCompleto);
            await context.SaveChangesAsync();

            TrabajoRepository repository = new TrabajoRepository(context);
            PaginationParams paginacion = new PaginationParams { PageNumber = 1, PageSize = 10 };

            Dtos.PagedResponse<Talleres360.Dtos.Trabajos.TrabajoDto> resultado = await repository.ObtenerTodosPagedAsync(
                1,
                paginacion,
                TrabajoEstado.ABIERTO,
                null,
                true);

            Assert.Single(resultado.Data);
            Assert.Equal(1, resultado.Data.First().Id);
        }

        [Fact]
        public async Task ObtenerDetallePorIdAsync_Debe_Retornar_Nulo_Si_Trabajo_Esta_Eliminado()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Trabajo trabajo = new Trabajo
            {
                Id = 20,
                TallerId = 1,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                DatosIncompletos = true,
                KmEntrada = 100,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = true
            };

            await context.Trabajos.AddAsync(trabajo);
            await context.SaveChangesAsync();

            TrabajoRepository repository = new TrabajoRepository(context);
            Talleres360.Dtos.Trabajos.TrabajoDto? detalle = await repository.ObtenerDetallePorIdAsync(20);

            Assert.Null(detalle);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Retornar_True_Cuando_Trabajo_Es_Valido()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Trabajo trabajo = new Trabajo
            {
                Id = 30,
                TallerId = 1,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                DatosIncompletos = true,
                KmEntrada = 100,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            await context.Trabajos.AddAsync(trabajo);
            await context.SaveChangesAsync();

            TrabajoRepository repository = new TrabajoRepository(context);
            bool pertenece = await repository.PerteneceATallerAsync(30, 1);

            Assert.True(pertenece);
        }

        [Fact]
        public async Task AddAsync_UpdateAsync_Y_ObtenerEntidadPorIdAsync_Deben_Funcionar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            TrabajoRepository repository = new TrabajoRepository(context);

            Trabajo trabajo = new Trabajo
            {
                TallerId = 1,
                Estado = TrabajoEstado.ABIERTO,
                EstadoPago = TrabajoEstadoPago.PENDIENTE,
                DatosIncompletos = true,
                KmEntrada = 100,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            await repository.AddAsync(trabajo);
            trabajo.Estado = TrabajoEstado.EN_PROCESO;
            await repository.UpdateAsync(trabajo);

            Trabajo? entidad = await repository.ObtenerEntidadPorIdAsync(trabajo.Id);
            Assert.NotNull(entidad);
            Assert.Equal(TrabajoEstado.EN_PROCESO, entidad!.Estado);
        }

        private static ApplicationDbContext CrearContexto(string nombreDb)
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDb)
                .Options;

            ApplicationDbContext context = new ApplicationDbContext(options);
            return context;
        }
    }
}
