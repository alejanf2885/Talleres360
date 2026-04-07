using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Inventario;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class CategoriaProductoRepositoryTests
    {
        [Fact]
        public async Task ObtenerCategoriasAsync_Debe_Filtrar_Por_Taller_Y_Ordenar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.CategoriasProducto.AddRangeAsync(
                new CategoriaProducto { Id = 1, TallerId = 1, Nombre = "B", Eliminado = false },
                new CategoriaProducto { Id = 2, TallerId = 1, Nombre = "A", Eliminado = false },
                new CategoriaProducto { Id = 3, TallerId = 2, Nombre = "C", Eliminado = false });
            await context.SaveChangesAsync();

            CategoriaProductoRepository repository = new CategoriaProductoRepository(context);
            List<CategoriaProducto> resultado = await repository.ObtenerCategoriasAsync(1);

            Assert.Equal(2, resultado.Count);
            Assert.Equal("A", resultado[0].Nombre);
            Assert.Equal("B", resultado[1].Nombre);
        }

        [Fact]
        public async Task ExisteNombreAsync_Debe_Validar_Coincidencia_Y_Exclusion()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.CategoriasProducto.AddAsync(
                new CategoriaProducto { Id = 10, TallerId = 1, Nombre = "ACEITES", Eliminado = false });
            await context.SaveChangesAsync();

            CategoriaProductoRepository repository = new CategoriaProductoRepository(context);

            bool existeSinExcluir = await repository.ExisteNombreAsync(1, "aceites");
            bool existeExcluyendoMisma = await repository.ExisteNombreAsync(1, "ACEITES", 10);

            Assert.True(existeSinExcluir);
            Assert.False(existeExcluyendoMisma);
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
