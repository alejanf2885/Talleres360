using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Enums;
using Talleres360.Repositories.NotasVehiculo;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class NotaVehiculoRepositoryTests
    {
        [Fact]
        public async Task ObtenerPorVehiculoAsync_Debe_Ordenar_Por_FechaDesc_Y_Filtrar_Eliminadas()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            NotaVehiculo notaAntigua = new NotaVehiculo
            {
                Id = 1,
                TallerId = 1,
                VehiculoId = 5,
                Texto = "Nota antigua",
                Tipo = NotaVehiculoTipo.GENERAL,
                Resuelta = false,
                FechaCreacion = new DateTime(2026, 1, 1),
                Eliminado = false
            };

            NotaVehiculo notaReciente = new NotaVehiculo
            {
                Id = 2,
                TallerId = 1,
                VehiculoId = 5,
                Texto = "Nota reciente",
                Tipo = NotaVehiculoTipo.AVISO,
                Resuelta = false,
                FechaCreacion = new DateTime(2026, 2, 1),
                Eliminado = false
            };

            NotaVehiculo notaEliminada = new NotaVehiculo
            {
                Id = 3,
                TallerId = 1,
                VehiculoId = 5,
                Texto = "No debe salir",
                Tipo = NotaVehiculoTipo.CLIENTE,
                Resuelta = false,
                FechaCreacion = new DateTime(2026, 3, 1),
                Eliminado = true
            };

            await context.NotasVehiculo.AddRangeAsync(notaAntigua, notaReciente, notaEliminada);
            await context.SaveChangesAsync();

            NotaVehiculoRepository repository = new NotaVehiculoRepository(context);
            List<Talleres360.Dtos.NotasVehiculo.NotaVehiculoDto> resultado = await repository.ObtenerPorVehiculoAsync(1, 5);

            Assert.Equal(2, resultado.Count);
            Assert.Equal(2, resultado[0].Id);
            Assert.Equal(1, resultado[1].Id);
            Assert.Equal(NotaVehiculoTipo.AVISO, resultado[0].Tipo);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Fallar_Con_Nota_Eliminada()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            NotaVehiculo nota = new NotaVehiculo
            {
                Id = 10,
                TallerId = 1,
                VehiculoId = 2,
                Texto = "Nota",
                Tipo = NotaVehiculoTipo.GENERAL,
                Resuelta = false,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = true
            };

            await context.NotasVehiculo.AddAsync(nota);
            await context.SaveChangesAsync();

            NotaVehiculoRepository repository = new NotaVehiculoRepository(context);
            bool pertenece = await repository.PerteneceATallerAsync(10, 1);

            Assert.False(pertenece);
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
