using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Clientes;

namespace Talleres360.Test.Repositories
{
    public class CustomerRepositoryTests
    {
        [Fact]
        public async Task ExistsByEmailAsync_Debe_Respetar_Taller()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Clientes.AddRangeAsync(
                new Cliente { Id = 1, TallerId = 1, Nombre = "Ana", Telefono = "1", Email = "ana@mail.com", FechaCreacion = DateTime.UtcNow, Eliminado = false },
                new Cliente { Id = 2, TallerId = 2, Nombre = "Ana", Telefono = "2", Email = "ana@mail.com", FechaCreacion = DateTime.UtcNow, Eliminado = false });
            await context.SaveChangesAsync();

            CustomerRepository repository = new CustomerRepository(context);

            bool existeTaller1 = await repository.ExistsByEmailAsync(1, "ana@mail.com");
            bool existeTaller3 = await repository.ExistsByEmailAsync(3, "ana@mail.com");

            Assert.True(existeTaller1);
            Assert.False(existeTaller3);
        }

        [Fact]
        public async Task CountNuevosEsteMesAsync_Debe_Contar_Solo_El_Mes_Actual()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Clientes.AddRangeAsync(
                new Cliente { Id = 1, TallerId = 1, Nombre = "Reciente", Telefono = "1", Email = "r@mail.com", FechaCreacion = DateTime.UtcNow, Eliminado = false },
                new Cliente { Id = 2, TallerId = 1, Nombre = "Antiguo", Telefono = "2", Email = "a@mail.com", FechaCreacion = DateTime.UtcNow.AddMonths(-2), Eliminado = false });
            await context.SaveChangesAsync();

            CustomerRepository repository = new CustomerRepository(context);
            int total = await repository.CountNuevosEsteMesAsync(1);

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetAllByTallerIdAsync_Debe_Aplicar_Busqueda()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Clientes.AddRangeAsync(
                new Cliente { Id = 1, TallerId = 1, Nombre = "Ana", Apellidos = "L�pez", Telefono = "111", Email = "ana@mail.com", FechaCreacion = DateTime.UtcNow, Eliminado = false },
                new Cliente { Id = 2, TallerId = 1, Nombre = "Pedro", Apellidos = "Ruiz", Telefono = "222", Email = "pedro@mail.com", FechaCreacion = DateTime.UtcNow, Eliminado = false });
            await context.SaveChangesAsync();

            CustomerRepository repository = new CustomerRepository(context);
            IEnumerable<Cliente> resultado = await repository.GetAllByTallerIdAsync(1, "ana");

            Assert.Single(resultado);
            Assert.Equal("Ana", resultado.First().Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_Y_PerteneceATallerAsync_Deben_Responder_Correctamente()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Clientes.AddAsync(
                new Cliente { Id = 7, TallerId = 1, Nombre = "Cliente", Telefono = "333", Email = "c@mail.com", FechaCreacion = DateTime.UtcNow, Eliminado = false });
            await context.SaveChangesAsync();

            CustomerRepository repository = new CustomerRepository(context);
            Cliente? cliente = await repository.GetByIdAsync(7, 1);
            bool pertenece = await repository.PerteneceATallerAsync(7, 1);

            Assert.NotNull(cliente);
            Assert.True(pertenece);
        }

        [Fact]
        public async Task AddAsync_Y_UpdateAsync_Deben_Persistir_Cambios()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            CustomerRepository repository = new CustomerRepository(context);

            Cliente cliente = new Cliente
            {
                TallerId = 1,
                Nombre = "Inicial",
                Telefono = "444",
                Email = "i@mail.com",
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            await repository.AddAsync(cliente);
            cliente.Nombre = "Actualizado";
            await repository.UpdateAsync(cliente);

            Cliente? guardado = await context.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(item => item.Id == cliente.Id);
            Assert.NotNull(guardado);
            Assert.Equal("Actualizado", guardado!.Nombre);
        }

        [Fact]
        public async Task ExistsByNifAsync_Debe_Normalizar_Entrada()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Clientes.AddAsync(
                new Cliente { Id = 9, TallerId = 1, Nombre = "Nif", Telefono = "555", Email = "nif@mail.com", NifCif = "12345678A", FechaCreacion = DateTime.UtcNow, Eliminado = false });
            await context.SaveChangesAsync();

            CustomerRepository repository = new CustomerRepository(context);
            bool existe = await repository.ExistsByNifAsync(1, " 12345678a ");

            Assert.True(existe);
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
