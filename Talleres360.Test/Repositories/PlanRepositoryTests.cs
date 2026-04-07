using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Planes;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class PlanRepositoryTests
    {
        [Fact]
        public async Task GetPlanPorNombreAsync_Debe_Encontrar_Plan()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Planes.AddAsync(new Plan { Id = 1, Nombre = "BASICO", PrecioMensual = 10, PrecioAnual = 100 });
            await context.SaveChangesAsync();

            PlanRepository repository = new PlanRepository(context);
            Plan? plan = await repository.GetPlanPorNombreAsync("BASICO");

            Assert.NotNull(plan);
            Assert.Equal(1, plan!.Id);
        }

        [Fact]
        public async Task GetPlanesActivosAsync_Debe_Devolver_Todos_Los_Planes()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Planes.AddRangeAsync(
                new Plan { Id = 1, Nombre = "BASICO", PrecioMensual = 10, PrecioAnual = 100, Activo = true },
                new Plan { Id = 2, Nombre = "PRO", PrecioMensual = 20, PrecioAnual = 200, Activo = false });
            await context.SaveChangesAsync();

            PlanRepository repository = new PlanRepository(context);
            IEnumerable<Plan> planes = await repository.GetPlanesActivosAsync();

            Assert.Equal(2, planes.Count());
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
