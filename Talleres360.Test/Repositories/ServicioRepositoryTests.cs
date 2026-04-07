using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Repositories.Servicios;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class ServicioRepositoryTests
    {
        [Fact]
        public async Task ObtenerTodosPagedAsync_Debe_Filtrar_Por_Taller_Busqueda_Y_Activo()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Servicios.AddRangeAsync(
                new Servicio { Id = 1, TallerId = 1, Nombre = "ALINEACION", PrecioBase = 40, ImpuestoPorcentaje = 21, Activo = true, Eliminado = false },
                new Servicio { Id = 2, TallerId = 1, Nombre = "CAMBIO ACEITE", PrecioBase = 50, ImpuestoPorcentaje = 21, Activo = false, Eliminado = false },
                new Servicio { Id = 3, TallerId = 2, Nombre = "ALINEACION", PrecioBase = 60, ImpuestoPorcentaje = 21, Activo = true, Eliminado = false });
            await context.SaveChangesAsync();

            ServicioRepository repository = new ServicioRepository(context);
            PaginationParams paginacion = new PaginationParams { PageNumber = 1, PageSize = 10 };

            Dtos.PagedResponse<Talleres360.Dtos.Servicios.ServicioDto> resultado = await repository.ObtenerTodosPagedAsync(1, paginacion, "aline", true);

            Assert.Single(resultado.Data);
            Assert.Equal("ALINEACION", resultado.Data.First().Nombre);
        }

        [Fact]
        public async Task ExisteNombreAsync_Debe_Respetar_Exclusion()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Servicios.AddAsync(new Servicio { Id = 10, TallerId = 1, Nombre = "DIAGNOSIS", PrecioBase = 30, ImpuestoPorcentaje = 21, Activo = true, Eliminado = false });
            await context.SaveChangesAsync();

            ServicioRepository repository = new ServicioRepository(context);

            bool existeSinExcluir = await repository.ExisteNombreAsync(1, " diagnosis ");
            bool existeExcluyendo = await repository.ExisteNombreAsync(1, "DIAGNOSIS", 10);

            Assert.True(existeSinExcluir);
            Assert.False(existeExcluyendo);
        }

        [Fact]
        public async Task AddUpdateObtenerDetalleYPertenece_Deben_Funcionar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            ServicioRepository repository = new ServicioRepository(context);

            Servicio servicio = new Servicio
            {
                TallerId = 1,
                Nombre = "PINTURA",
                PrecioBase = 100,
                ImpuestoPorcentaje = 21,
                Activo = true,
                Eliminado = false
            };

            await repository.AddAsync(servicio);

            servicio.Nombre = "PINTURA PREMIUM";
            await repository.UpdateAsync(servicio);

            Servicio? entidad = await repository.ObtenerEntidadPorIdAsync(servicio.Id);
            Talleres360.Dtos.Servicios.ServicioDto? detalle = await repository.ObtenerDetallePorIdAsync(servicio.Id);
            bool pertenece = await repository.PerteneceATallerAsync(servicio.Id, 1);

            Assert.NotNull(entidad);
            Assert.Equal("PINTURA PREMIUM", entidad!.Nombre);
            Assert.NotNull(detalle);
            Assert.True(pertenece);
        }

        private static ApplicationDbContext CrearContexto(string nombreDb)
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDb)
                .Options;
            return new ApplicationDbContext(options);
        }
    }
}
