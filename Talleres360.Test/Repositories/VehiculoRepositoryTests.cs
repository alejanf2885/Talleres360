using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Vehiculos;

namespace Talleres360.Test.Repositories
{
    public class VehiculoRepositoryTests
    {
        [Fact]
        public async Task ExistsAsync_Debe_Validar_Matricula_No_Eliminada()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Vehiculos.AddRangeAsync(
                new Vehiculo { Id = 1, TallerId = 1, VehiculoTipoId = 1, MarcaId = 1, ModeloId = 1, Matricula = "1234ABC", Eliminado = false, FechaCreacion = DateTime.UtcNow },
                new Vehiculo { Id = 2, TallerId = 1, VehiculoTipoId = 1, MarcaId = 1, ModeloId = 1, Matricula = "9999ZZZ", Eliminado = true, FechaCreacion = DateTime.UtcNow });
            await context.SaveChangesAsync();

            VehiculoRepository repository = new VehiculoRepository(context);
            bool existe = await repository.ExistsAsync("1234ABC", 1);
            bool existeEliminado = await repository.ExistsAsync("9999ZZZ", 1);

            Assert.True(existe);
            Assert.False(existeEliminado);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Retornar_True_Cuando_Coincide_Taller()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Vehiculos.AddAsync(new Vehiculo { Id = 7, TallerId = 1, VehiculoTipoId = 1, MarcaId = 1, ModeloId = 1, Matricula = "7777AAA", Eliminado = false, FechaCreacion = DateTime.UtcNow });
            await context.SaveChangesAsync();

            VehiculoRepository repository = new VehiculoRepository(context);
            bool pertenece = await repository.PerteneceATallerAsync(7, 1);

            Assert.True(pertenece);
        }

        [Fact]
        public async Task UpdateAsync_Debe_Persistir_Cambios_En_Vehiculo()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Vehiculo vehiculo = new Vehiculo
            {
                Id = 9,
                TallerId = 1,
                VehiculoTipoId = 1,
                MarcaId = 1,
                ModeloId = 1,
                Matricula = "9999CCC",
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            await context.Vehiculos.AddAsync(vehiculo);
            await context.SaveChangesAsync();

            VehiculoRepository repository = new VehiculoRepository(context);
            vehiculo.KmActuales = 12345;
            await repository.UpdateAsync(vehiculo);

            Vehiculo? actualizado = await context.Vehiculos.IgnoreQueryFilters().FirstOrDefaultAsync(item => item.Id == 9);
            Assert.NotNull(actualizado);
            Assert.Equal(12345, actualizado!.KmActuales);
        }

        [Fact]
        public async Task AddAsync_Y_GetByIdAsync_Deben_Funcionar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            VehiculoRepository repository = new VehiculoRepository(context);

            Vehiculo vehiculo = new Vehiculo
            {
                TallerId = 1,
                VehiculoTipoId = 1,
                MarcaId = 1,
                ModeloId = 1,
                Matricula = "5555DDD",
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            await repository.AddAsync(vehiculo);
            Vehiculo? cargado = await repository.GetByIdAsync(vehiculo.Id, 1);

            Assert.NotNull(cargado);
            Assert.Equal("5555DDD", cargado!.Matricula);
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
