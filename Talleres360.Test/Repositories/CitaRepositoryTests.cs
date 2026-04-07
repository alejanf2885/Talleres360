using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Enums;
using Talleres360.Repositories.Citas;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class CitaRepositoryTests
    {
        [Fact]
        public async Task ObtenerTodasPagedAsync_Debe_Filtrar_Por_Estado_Vehiculo_Y_Fecha()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Cita citaValida = new Cita
            {
                Id = 1,
                TallerId = 1,
                VehiculoId = 10,
                Estado = CitaEstado.PENDIENTE,
                FechaCita = new DateTime(2026, 01, 15),
                Eliminado = false
            };

            Cita citaEstadoDistinto = new Cita
            {
                Id = 2,
                TallerId = 1,
                VehiculoId = 10,
                Estado = CitaEstado.CONFIRMADA,
                FechaCita = new DateTime(2026, 01, 15),
                Eliminado = false
            };

            Cita citaOtroVehiculo = new Cita
            {
                Id = 3,
                TallerId = 1,
                VehiculoId = 99,
                Estado = CitaEstado.PENDIENTE,
                FechaCita = new DateTime(2026, 01, 15),
                Eliminado = false
            };

            await context.Citas.AddRangeAsync(citaValida, citaEstadoDistinto, citaOtroVehiculo);
            await context.SaveChangesAsync();

            CitaRepository repository = new CitaRepository(context);
            PaginationParams paginacion = new PaginationParams { PageNumber = 1, PageSize = 10 };

            Dtos.PagedResponse<Talleres360.Dtos.Citas.CitaDto> resultado = await repository.ObtenerTodasPagedAsync(
                1,
                paginacion,
                new DateTime(2026, 01, 01),
                new DateTime(2026, 01, 31),
                CitaEstado.PENDIENTE,
                10);

            Assert.Single(resultado.Data);
            Assert.Equal(1, resultado.Data.First().Id);
        }

        [Fact]
        public async Task ObtenerDetallePorIdAsync_Debe_Omitir_Nota_Eliminada()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Cita cita = new Cita
            {
                Id = 20,
                TallerId = 1,
                VehiculoId = 10,
                Estado = CitaEstado.PENDIENTE,
                FechaCita = DateTime.UtcNow,
                Eliminado = true
            };

            await context.Citas.AddAsync(cita);
            await context.SaveChangesAsync();

            CitaRepository repository = new CitaRepository(context);
            Talleres360.Dtos.Citas.CitaDto? detalle = await repository.ObtenerDetallePorIdAsync(20);

            Assert.Null(detalle);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Retornar_True_Cuando_Cita_Existe_Y_No_Esta_Eliminada()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Cita cita = new Cita
            {
                Id = 30,
                TallerId = 1,
                VehiculoId = 10,
                Estado = CitaEstado.PENDIENTE,
                FechaCita = DateTime.UtcNow,
                Eliminado = false
            };

            await context.Citas.AddAsync(cita);
            await context.SaveChangesAsync();

            CitaRepository repository = new CitaRepository(context);
            bool pertenece = await repository.PerteneceATallerAsync(30, 1);

            Assert.True(pertenece);
        }

        [Fact]
        public async Task AddAsync_UpdateAsync_Y_ObtenerEntidadPorIdAsync_Deben_Funcionar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            CitaRepository repository = new CitaRepository(context);

            Cita cita = new Cita
            {
                TallerId = 1,
                VehiculoId = 10,
                Estado = CitaEstado.PENDIENTE,
                FechaCita = DateTime.UtcNow,
                Eliminado = false
            };

            await repository.AddAsync(cita);
            cita.Estado = CitaEstado.CONFIRMADA;
            await repository.UpdateAsync(cita);

            Cita? entidad = await repository.ObtenerEntidadPorIdAsync(cita.Id);
            Assert.NotNull(entidad);
            Assert.Equal(CitaEstado.CONFIRMADA, entidad!.Estado);
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
