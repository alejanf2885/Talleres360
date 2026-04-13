using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Models;
using Talleres360.Repositories.Trabajos;

namespace Talleres360.Test.Repositories
{
    public class CobroTrabajoRepositoryTests
    {
        [Fact]
        public async Task ObtenerTodosPorTrabajoPagedAsync_Debe_Retornar_Cobros_Ordenados_Por_FechaCobro()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro1 = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 100,
                MetodoPago = CobroMetodoPago.EFECTIVO,
                FechaCobro = new DateTime(2026, 4, 10),
                Eliminado = false
            };

            CobroTrabajo cobro2 = new CobroTrabajo
            {
                Id = 2,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 200,
                MetodoPago = CobroMetodoPago.TARJETA,
                FechaCobro = new DateTime(2026, 4, 12),
                Eliminado = false
            };

            CobroTrabajo cobro3 = new CobroTrabajo
            {
                Id = 3,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 150,
                MetodoPago = CobroMetodoPago.TRANSFERENCIA,
                FechaCobro = new DateTime(2026, 4, 11),
                Eliminado = false
            };

            await context.CobrosTrabajo.AddRangeAsync(cobro1, cobro2, cobro3);
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            PaginationParams paginacion = new PaginationParams { PageNumber = 1, PageSize = 10 };

            PagedResponse<CobroTrabajoDto> resultado = await repository.ObtenerTodosPorTrabajoPagedAsync(1, paginacion);

            Assert.Equal(3, resultado.TotalCount);
            Assert.Equal(3, resultado.Data.Count());
            // Should be ordered by FechaCobro DESC
            Assert.Equal(2, resultado.Data.First().Id); // 2026-04-12
            Assert.Equal(3, resultado.Data.ElementAt(1).Id); // 2026-04-11
            Assert.Equal(1, resultado.Data.ElementAt(2).Id); // 2026-04-10
        }

        [Fact]
        public async Task ObtenerTodosPorTrabajoPagedAsync_Debe_Excluir_Cobros_Eliminados()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobroActivo = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 100,
                FechaCobro = DateTime.UtcNow,
                Eliminado = false
            };

            CobroTrabajo cobroEliminado = new CobroTrabajo
            {
                Id = 2,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 200,
                FechaCobro = DateTime.UtcNow,
                Eliminado = true
            };

            await context.CobrosTrabajo.AddRangeAsync(cobroActivo, cobroEliminado);
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            PaginationParams paginacion = new PaginationParams { PageNumber = 1, PageSize = 10 };

            PagedResponse<CobroTrabajoDto> resultado = await repository.ObtenerTodosPorTrabajoPagedAsync(1, paginacion);

            Assert.Single(resultado.Data);
            Assert.Equal(1, resultado.Data.First().Id);
            Assert.Equal(1, resultado.TotalCount);
        }

        [Fact]
        public async Task ObtenerTodosPorTrabajoPagedAsync_Debe_Aplicar_Paginacion()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            for (int i = 1; i <= 25; i++)
            {
                CobroTrabajo cobro = new CobroTrabajo
                {
                    Id = i,
                    TallerId = 1,
                    TrabajoId = 1,
                    Importe = i * 10,
                    FechaCobro = DateTime.UtcNow.AddDays(-i),
                    Eliminado = false
                };
                await context.CobrosTrabajo.AddAsync(cobro);
            }
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);

            // Page 1, 10 items per page
            PaginationParams paginacion1 = new PaginationParams { PageNumber = 1, PageSize = 10 };
            PagedResponse<CobroTrabajoDto> resultado1 = await repository.ObtenerTodosPorTrabajoPagedAsync(1, paginacion1);

            Assert.Equal(10, resultado1.Data.Count());
            Assert.Equal(25, resultado1.TotalCount);

            // Page 2, 10 items per page
            PaginationParams paginacion2 = new PaginationParams { PageNumber = 2, PageSize = 10 };
            PagedResponse<CobroTrabajoDto> resultado2 = await repository.ObtenerTodosPorTrabajoPagedAsync(1, paginacion2);

            Assert.Equal(10, resultado2.Data.Count());
            Assert.Equal(25, resultado2.TotalCount);

            // Page 3, 10 items per page
            PaginationParams paginacion3 = new PaginationParams { PageNumber = 3, PageSize = 10 };
            PagedResponse<CobroTrabajoDto> resultado3 = await repository.ObtenerTodosPorTrabajoPagedAsync(1, paginacion3);

            Assert.Equal(5, resultado3.Data.Count());
            Assert.Equal(25, resultado3.TotalCount);
        }

        [Fact]
        public async Task ObtenerEntidadPorIdAsync_Debe_Retornar_Cobro_Existente()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 500,
                MetodoPago = CobroMetodoPago.TARJETA,
                FechaCobro = DateTime.UtcNow,
                Eliminado = false
            };

            await context.CobrosTrabajo.AddAsync(cobro);
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            CobroTrabajo? resultado = await repository.ObtenerEntidadPorIdAsync(1);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.Id);
            Assert.Equal(500, resultado.Importe);
        }

        [Fact]
        public async Task ObtenerEntidadPorIdAsync_Debe_Retornar_Nulo_Para_Cobro_Inexistente()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);

            CobroTrabajo? resultado = await repository.ObtenerEntidadPorIdAsync(999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObtenerTotalCobradoAsync_Debe_Sumar_Solo_Cobros_Activos()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro1 = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 100,
                FechaCobro = DateTime.UtcNow,
                Eliminado = false
            };

            CobroTrabajo cobro2 = new CobroTrabajo
            {
                Id = 2,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 200,
                FechaCobro = DateTime.UtcNow,
                Eliminado = false
            };

            CobroTrabajo cobroEliminado = new CobroTrabajo
            {
                Id = 3,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 300,
                FechaCobro = DateTime.UtcNow,
                Eliminado = true
            };

            await context.CobrosTrabajo.AddRangeAsync(cobro1, cobro2, cobroEliminado);
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            decimal total = await repository.ObtenerTotalCobradoAsync(1);

            // Should only sum cobro1 and cobro2 (not the deleted one)
            Assert.Equal(300, total);
        }

        [Fact]
        public async Task ObtenerTotalCobradoAsync_Debe_Retornar_Cero_Cuando_No_Hay_Cobros()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);

            decimal total = await repository.ObtenerTotalCobradoAsync(1);

            Assert.Equal(0, total);
        }

        [Fact]
        public async Task AddAsync_Debe_Guardar_Cobro_En_Base_Datos()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro = new CobroTrabajo
            {
                TallerId = 1,
                TrabajoId = 1,
                Importe = 500,
                MetodoPago = CobroMetodoPago.TRANSFERENCIA,
                Referencia = "TXN-123",
                FechaCobro = DateTime.UtcNow,
                Eliminado = false
            };

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            await repository.AddAsync(cobro);

            // Verify it was saved
            Assert.True(cobro.Id > 0);

            CobroTrabajo? guardado = await context.CobrosTrabajo.FindAsync(cobro.Id);
            Assert.NotNull(guardado);
            Assert.Equal(500, guardado.Importe);
            Assert.Equal("TXN-123", guardado.Referencia);
        }

        [Fact]
        public async Task UpdateAsync_Debe_Actualizar_Cobro_En_Base_Datos()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 500,
                Notas = "Original",
                Eliminado = false
            };

            await context.CobrosTrabajo.AddAsync(cobro);
            await context.SaveChangesAsync();

            // Modify and update
            cobro.Notas = "Actualizado";
            cobro.Eliminado = true;

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            await repository.UpdateAsync(cobro);

            // Verify update
            CobroTrabajo? actualizado = await context.CobrosTrabajo.FindAsync(1);
            Assert.NotNull(actualizado);
            Assert.Equal("Actualizado", actualizado.Notas);
            Assert.True(actualizado.Eliminado);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Retornar_True_Para_Cobro_Valido()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 500,
                FechaCobro = DateTime.UtcNow,
                Eliminado = false
            };

            await context.CobrosTrabajo.AddAsync(cobro);
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            bool pertenece = await repository.PerteneceATallerAsync(1, 1);

            Assert.True(pertenece);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Retornar_False_Para_Cobro_De_Otro_Taller()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 500,
                FechaCobro = DateTime.UtcNow,
                Eliminado = false
            };

            await context.CobrosTrabajo.AddAsync(cobro);
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            bool pertenece = await repository.PerteneceATallerAsync(1, 99); // Different workshop

            Assert.False(pertenece);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Retornar_False_Para_Cobro_Eliminado()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CobroTrabajo cobro = new CobroTrabajo
            {
                Id = 1,
                TallerId = 1,
                TrabajoId = 1,
                Importe = 500,
                FechaCobro = DateTime.UtcNow,
                Eliminado = true
            };

            await context.CobrosTrabajo.AddAsync(cobro);
            await context.SaveChangesAsync();

            CobroTrabajoRepository repository = new CobroTrabajoRepository(context);
            bool pertenece = await repository.PerteneceATallerAsync(1, 1);

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
