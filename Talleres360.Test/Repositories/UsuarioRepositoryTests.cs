using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Usuarios;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class UsuarioRepositoryTests
    {
        [Fact]
        public async Task ExisteEmailAsync_Debe_Encontrar_Usuario_Eliminado_Por_IgnoreQueryFilters()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Usuarios.AddAsync(new Usuario
            {
                Id = 1,
                Nombre = "Usuario",
                Email = "usuario@mail.com",
                Rol = global::Talleres360.Enum.RolesUsuario.ADMIN,
                Activo = false,
                Eliminado = true,
                FechaCreacion = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            UsuarioRepository repository = new UsuarioRepository(context);
            bool existe = await repository.ExisteEmailAsync("usuario@mail.com");

            Assert.True(existe);
        }

        [Fact]
        public async Task SaveChangesAsync_Debe_Persistir_Usuario_En_AddAsync()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            UsuarioRepository repository = new UsuarioRepository(context);

            Usuario usuario = new Usuario
            {
                Nombre = "Nuevo",
                Email = "nuevo@mail.com",
                Rol = global::Talleres360.Enum.RolesUsuario.ADMIN,
                Activo = false,
                Eliminado = false,
                FechaCreacion = DateTime.UtcNow
            };

            await repository.AddAsync(usuario);
            await repository.SaveChangesAsync();

            Usuario? guardado = await context.Usuarios.FirstOrDefaultAsync(item => item.Email == "nuevo@mail.com");
            Assert.NotNull(guardado);
        }

        [Fact]
        public async Task GetCredencialLocalByUsuarioIdAsync_Debe_Retornar_Solo_Local_No_Eliminada()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Credenciales.AddRangeAsync(
                new Credencial { Id = 1, UsuarioId = 50, TipoInicioSesion = "LOCAL", PasswordHash = "hash", Eliminado = false },
                new Credencial { Id = 2, UsuarioId = 50, TipoInicioSesion = "GOOGLE", ProviderKey = "pk", Eliminado = false });
            await context.SaveChangesAsync();

            UsuarioRepository repository = new UsuarioRepository(context);
            Credencial? credencial = await repository.GetCredencialLocalByUsuarioIdAsync(50);

            Assert.NotNull(credencial);
            Assert.Equal("LOCAL", credencial!.TipoInicioSesion);
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
