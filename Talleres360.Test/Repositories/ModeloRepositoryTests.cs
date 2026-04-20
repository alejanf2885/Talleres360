using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class ModeloRepositoryTests
    {
        [Fact]
        public async Task ObtenerModelosPorMarcaAsync_Debe_Traer_Oficiales_Y_Del_Taller()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Modelos.AddRangeAsync(
                new Modelo { Id = 1, MarcaId = 10, VehiculoTipoId = 1, Nombre = "COROLLA", EsOficial = true, TallerId = null },
                new Modelo { Id = 2, MarcaId = 10, VehiculoTipoId = 1, Nombre = "LOCAL", EsOficial = false, TallerId = 1 },
                new Modelo { Id = 3, MarcaId = 10, VehiculoTipoId = 1, Nombre = "OTRO", EsOficial = false, TallerId = 2 });
            await context.SaveChangesAsync();

            ModeloRepository repository = new ModeloRepository(context);
            List<Talleres360.Dtos.Vehiculos.ModeloVehiculoDto> resultado = await repository.ObtenerModelosPorMarcaAsync(1, 10);

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, item => item.Nombre == "COROLLA");
            Assert.Contains(resultado, item => item.Nombre == "LOCAL");
        }

        [Fact]
        public async Task TieneDependenciasAsync_Debe_Retornar_True_Cuando_Hay_Vehiculos()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Modelos.AddAsync(new Modelo { Id = 20, MarcaId = 1, VehiculoTipoId = 1, Nombre = "MODELO-X", TallerId = 1, EsOficial = false });
            await context.Vehiculos.AddAsync(new Vehiculo
            {
                Id = 40,
                TallerId = 1,
                VehiculoTipoId = 1,
                MarcaId = 1,
                ModeloId = 20,
                Matricula = "2222BBB",
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            });
            await context.SaveChangesAsync();

            ModeloRepository repository = new ModeloRepository(context);
            bool tieneDependencias = await repository.TieneDependenciasAsync(20);

            Assert.True(tieneDependencias);
        }

        [Fact]
        public async Task GetByIdAsync_Debe_Retornar_Modelo_Por_Id()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Modelos.AddAsync(new Modelo
            {
                Id = 50,
                MarcaId = 1,
                VehiculoTipoId = 1,
                Nombre = "IBIZA",
                EsOficial = true,
                TallerId = null
            });
            await context.SaveChangesAsync();

            ModeloRepository repository = new ModeloRepository(context);
            Modelo? modelo = await repository.GetByIdAsync(50, 1);

            Assert.NotNull(modelo);
            Assert.Equal("IBIZA", modelo!.Nombre);
        }

        [Fact]
        public async Task ExisteModeloOficialAsync_Y_ExisteModeloEnTallerAsync_Deben_Normalizar_Nombre()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Modelos.AddRangeAsync(
                new Modelo { Id = 60, MarcaId = 7, VehiculoTipoId = 1, Nombre = "CORSA", EsOficial = true, TallerId = null },
                new Modelo { Id = 61, MarcaId = 7, VehiculoTipoId = 1, Nombre = "CORSA_LOCAL", EsOficial = false, TallerId = 1 });
            await context.SaveChangesAsync();

            ModeloRepository repository = new ModeloRepository(context);

            bool oficial = await repository.ExisteModeloOficialAsync(7, " corsa ");
            bool enTaller = await repository.ExisteModeloEnTallerAsync(7, " corsa_local ", 1);
            bool noEnTaller = await repository.ExisteModeloEnTallerAsync(7, "corsa_local", 2);

            Assert.True(oficial);
            Assert.True(enTaller);
            Assert.False(noEnTaller);
        }

        [Fact]
        public async Task AddUpdateDelete_Y_PerteneceATallerAsync_Deben_Funcionar_Correctamente()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            ModeloRepository repository = new ModeloRepository(context);

            Modelo modelo = new Modelo
            {
                MarcaId = 1,
                VehiculoTipoId = 1,
                Nombre = "NUEVO",
                TallerId = 1,
                EsOficial = false
            };

            await repository.AddAsync(modelo);

            bool perteneceAntes = await repository.PerteneceATallerAsync(modelo.Id, 1);
            Assert.True(perteneceAntes);

            modelo.Nombre = "EDITADO";
            await repository.UpdateAsync(modelo);

            Modelo? actualizado = await context.Modelos.FirstOrDefaultAsync(item => item.Id == modelo.Id);
            Assert.NotNull(actualizado);
            Assert.Equal("EDITADO", actualizado!.Nombre);

            await repository.DeleteAsync(modelo);
            Modelo? eliminado = await context.Modelos.FirstOrDefaultAsync(item => item.Id == modelo.Id);
            Assert.Null(eliminado);
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
