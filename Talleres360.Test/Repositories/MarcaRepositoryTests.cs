using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class MarcaRepositoryTests
    {
        [Fact]
        public async Task ObtenerMarcasAsync_Debe_Incluir_Oficiales_Y_Del_Taller()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Marcas.AddRangeAsync(
                new Marca { Id = 1, Nombre = "TOYOTA", EsOficial = true, TallerId = null },
                new Marca { Id = 2, Nombre = "LOCAL", EsOficial = false, TallerId = 1 },
                new Marca { Id = 3, Nombre = "OTRO", EsOficial = false, TallerId = 2 });
            await context.SaveChangesAsync();

            MarcaRepository repository = new MarcaRepository(context);
            List<Talleres360.Dtos.Vehiculos.MarcaVehiculoDto> resultado = await repository.ObtenerMarcasAsync(1);

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, item => item.Nombre == "TOYOTA");
            Assert.Contains(resultado, item => item.Nombre == "LOCAL");
        }

        [Fact]
        public async Task TieneDependenciasAsync_Debe_Retornar_True_Cuando_Hay_Vehiculos()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Marcas.AddAsync(new Marca { Id = 7, Nombre = "DEPENDENCIA", EsOficial = false, TallerId = 1 });
            await context.Vehiculos.AddAsync(new Vehiculo
            {
                Id = 11,
                TallerId = 1,
                VehiculoTipoId = 1,
                MarcaId = 7,
                ModeloId = 1,
                Matricula = "1111AAA",
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            });
            await context.SaveChangesAsync();

            MarcaRepository repository = new MarcaRepository(context);
            bool tieneDependencias = await repository.TieneDependenciasAsync(7);

            Assert.True(tieneDependencias);
        }

        [Fact]
        public async Task ExisteMarcaVisibleAsync_Debe_Retornar_True_Para_Oficial_O_Del_Taller()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Marcas.AddRangeAsync(
                new Marca { Id = 10, Nombre = "OFICIAL", EsOficial = true, TallerId = null },
                new Marca { Id = 11, Nombre = "LOCAL", EsOficial = false, TallerId = 1 },
                new Marca { Id = 12, Nombre = "OTRO", EsOficial = false, TallerId = 2 });
            await context.SaveChangesAsync();

            MarcaRepository repository = new MarcaRepository(context);

            bool oficialVisible = await repository.ExisteMarcaVisibleAsync(1, 10);
            bool localVisible = await repository.ExisteMarcaVisibleAsync(1, 11);
            bool otroNoVisible = await repository.ExisteMarcaVisibleAsync(1, 12);

            Assert.True(oficialVisible);
            Assert.True(localVisible);
            Assert.False(otroNoVisible);
        }

        [Fact]
        public async Task GetMarcaByIdAsync_Y_GetMarcaVisibleByNombreAsync_Deben_Funcionar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Marcas.AddRangeAsync(
                new Marca { Id = 20, Nombre = "SEAT", EsOficial = true, TallerId = null },
                new Marca { Id = 21, Nombre = "PERSONALIZADA", EsOficial = false, TallerId = 1 });
            await context.SaveChangesAsync();

            MarcaRepository repository = new MarcaRepository(context);

            Marca? porId = await repository.GetMarcaByIdAsync(21);
            Marca? visiblePorNombreOficial = await repository.GetMarcaVisibleByNombreAsync(1, "seat");
            Marca? visiblePorNombreTaller = await repository.GetMarcaVisibleByNombreAsync(1, "PERSONALIZADA");

            Assert.NotNull(porId);
            Assert.Equal("PERSONALIZADA", porId!.Nombre);
            Assert.NotNull(visiblePorNombreOficial);
            Assert.NotNull(visiblePorNombreTaller);
        }

        [Fact]
        public async Task ExisteMarcaOficialAsync_Y_ExisteMarcaEnTallerAsync_Deben_Normalizar_Nombre()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Marcas.AddRangeAsync(
                new Marca { Id = 30, Nombre = "FORD", EsOficial = true, TallerId = null },
                new Marca { Id = 31, Nombre = "MI_MARCA", EsOficial = false, TallerId = 1 });
            await context.SaveChangesAsync();

            MarcaRepository repository = new MarcaRepository(context);

            bool oficial = await repository.ExisteMarcaOficialAsync(" ford ");
            bool enTaller = await repository.ExisteMarcaEnTallerAsync(" mi_marca ", 1);
            bool noEnTaller = await repository.ExisteMarcaEnTallerAsync("mi_marca", 2);

            Assert.True(oficial);
            Assert.True(enTaller);
            Assert.False(noEnTaller);
        }

        [Fact]
        public async Task AddUpdateDelete_Y_PerteneceATallerAsync_Deben_Funcionar_Correctamente()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            MarcaRepository repository = new MarcaRepository(context);

            Marca marca = new Marca
            {
                Nombre = "NUEVA",
                EsOficial = false,
                TallerId = 1
            };

            await repository.AddAsync(marca);

            bool perteneceAntes = await repository.PerteneceATallerAsync(marca.Id, 1);
            Assert.True(perteneceAntes);

            marca.Nombre = "EDITADA";
            await repository.UpdateAsync(marca);

            Marca? actualizada = await context.Marcas.FirstOrDefaultAsync(item => item.Id == marca.Id);
            Assert.NotNull(actualizada);
            Assert.Equal("EDITADA", actualizada!.Nombre);

            await repository.DeleteAsync(marca);
            Marca? eliminada = await context.Marcas.FirstOrDefaultAsync(item => item.Id == marca.Id);
            Assert.Null(eliminada);
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
