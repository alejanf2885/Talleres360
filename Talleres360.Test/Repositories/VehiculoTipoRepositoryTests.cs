using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class VehiculoTipoRepositoryTests
    {
        [Fact]
        public async Task ObtenerTiposVehiculoAsync_Debe_Ordenar_Por_Nombre()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.VehiculoTipos.AddRangeAsync(
                new VehiculoTipo { Id = 1, Nombre = "Moto" },
                new VehiculoTipo { Id = 2, Nombre = "Auto" });
            await context.SaveChangesAsync();

            VehiculoTipoRepository repository = new VehiculoTipoRepository(context);
            List<Talleres360.Dtos.Vehiculos.VehiculoTipoDto> resultado = await repository.ObtenerTiposVehiculoAsync();

            Assert.Equal(2, resultado.Count);
            Assert.Equal("Auto", resultado[0].Nombre);
            Assert.Equal("Moto", resultado[1].Nombre);
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
