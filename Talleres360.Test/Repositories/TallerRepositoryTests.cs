using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Talleres;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class TallerRepositoryTests
    {
        [Fact]
        public async Task ExistsByCifAsync_Debe_Retornar_True_Si_Existe()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Talleres.AddAsync(new Taller { Id = 1, Nombre = "Taller", Cif = "A123", EstadoSuscripcion = "ACTIVO" });
            await context.SaveChangesAsync();

            TallerRepository repository = new TallerRepository(context);
            bool existe = await repository.ExistsByCifAsync("A123");

            Assert.True(existe);
        }

        [Fact]
        public async Task IsPerfilConfiguradoAsync_Debe_Leer_Valor_Real()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Talleres.AddAsync(new Taller
            {
                Id = 5,
                Nombre = "Taller Perfil",
                Cif = "B123",
                EstadoSuscripcion = "ACTIVO",
                PerfilConfigurado = true
            });
            await context.SaveChangesAsync();

            TallerRepository repository = new TallerRepository(context);
            bool perfilConfigurado = await repository.IsPerfilConfiguradoAsync(5);

            Assert.True(perfilConfigurado);
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
