using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Repositories.Inventario;

namespace Talleres360.Test.Repositories
{
    public class ProductoRepositoryTests
    {
        [Fact]
        public async Task ObtenerProductosPagedAsync_Debe_Filtrar_Por_Taller_Categoria_Y_Busqueda()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            CategoriaProducto categoriaUno = new CategoriaProducto { Id = 1, TallerId = 1, Nombre = "ACEITES", Eliminado = false };
            CategoriaProducto categoriaDos = new CategoriaProducto { Id = 2, TallerId = 1, Nombre = "FILTROS", Eliminado = false };
            CategoriaProducto categoriaOtroTaller = new CategoriaProducto { Id = 3, TallerId = 2, Nombre = "ACEITES", Eliminado = false };

            await context.CategoriasProducto.AddRangeAsync(categoriaUno, categoriaDos, categoriaOtroTaller);

            Producto productoUno = new Producto
            {
                Id = 1,
                TallerId = 1,
                CategoriaId = 1,
                Nombre = "ACEITE 5W30",
                Referencia = "AC-001",
                PrecioCompra = 10,
                PrecioVenta = 15,
                StockActual = 5,
                ControlarStock = true,
                Eliminado = false
            };

            Producto productoDos = new Producto
            {
                Id = 2,
                TallerId = 1,
                CategoriaId = 2,
                Nombre = "FILTRO AIRE",
                Referencia = "FI-001",
                PrecioCompra = 8,
                PrecioVenta = 12,
                StockActual = 2,
                ControlarStock = true,
                Eliminado = false
            };

            Producto productoOtroTaller = new Producto
            {
                Id = 3,
                TallerId = 2,
                CategoriaId = 3,
                Nombre = "ACEITE 10W40",
                Referencia = "AC-999",
                PrecioCompra = 9,
                PrecioVenta = 14,
                StockActual = 1,
                ControlarStock = true,
                Eliminado = false
            };

            await context.Productos.AddRangeAsync(productoUno, productoDos, productoOtroTaller);
            await context.SaveChangesAsync();

            ProductoRepository repository = new ProductoRepository(context);
            PaginationParams paginacion = new PaginationParams { PageNumber = 1, PageSize = 10 };

            PagedResponse<Talleres360.Dtos.Inventario.ProductoDto> resultado = await repository.ObtenerProductosPagedAsync(
                1,
                paginacion,
                "aceite",
                1);

            Assert.Single(resultado.Data);
            Assert.Equal("ACEITE 5W30", resultado.Data.First().Nombre);
            Assert.Equal(1, resultado.TotalCount);
        }

        [Fact]
        public async Task ExisteNombreAsync_Y_ExisteReferenciaAsync_Deben_Respetar_Exclusion()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Producto producto = new Producto
            {
                Id = 10,
                TallerId = 1,
                CategoriaId = 1,
                Nombre = "PASTILLAS FRENO",
                Referencia = "PF-100",
                PrecioCompra = 20,
                PrecioVenta = 30,
                StockActual = 4,
                ControlarStock = true,
                Eliminado = false
            };

            await context.Productos.AddAsync(producto);
            await context.SaveChangesAsync();

            ProductoRepository repository = new ProductoRepository(context);

            bool existeNombreSinExcluir = await repository.ExisteNombreAsync(1, "pastillas freno");
            bool existeNombreExcluyendoMismo = await repository.ExisteNombreAsync(1, "PASTILLAS FRENO", 10);
            bool existeReferenciaSinExcluir = await repository.ExisteReferenciaAsync(1, "pf-100");
            bool existeReferenciaExcluyendoMismo = await repository.ExisteReferenciaAsync(1, "PF-100", 10);

            Assert.True(existeNombreSinExcluir);
            Assert.False(existeNombreExcluyendoMismo);
            Assert.True(existeReferenciaSinExcluir);
            Assert.False(existeReferenciaExcluyendoMismo);
        }

        [Fact]
        public async Task PerteneceATallerAsync_Debe_Retornar_False_Para_Producto_Eliminado()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            Producto producto = new Producto
            {
                Id = 30,
                TallerId = 1,
                CategoriaId = 1,
                Nombre = "BUJIA",
                Referencia = "BJ-001",
                PrecioCompra = 4,
                PrecioVenta = 7,
                StockActual = 10,
                ControlarStock = true,
                Eliminado = true
            };

            await context.Productos.AddAsync(producto);
            await context.SaveChangesAsync();

            ProductoRepository repository = new ProductoRepository(context);

            bool pertenece = await repository.PerteneceATallerAsync(30, 1);

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
