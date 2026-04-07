using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Enums;
using Talleres360.Repositories;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class RefreshTokenRepositoryTests
    {
        [Fact]
        public async Task ObtenerPorTokenAsync_Debe_Retornar_Solo_RefreshToken()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.TokensSeguridad.AddRangeAsync(
                new TokenSeguridad { Id = 1, UsuarioId = 1, Token = "refresh", TipoToken = TipoTokenSeguridad.RefreshToken.ToDbValue(), FechaCreacion = DateTime.UtcNow, FechaExpiracion = DateTime.UtcNow.AddDays(1), Usado = false },
                new TokenSeguridad { Id = 2, UsuarioId = 1, Token = "reset", TipoToken = TipoTokenSeguridad.ResetPassword.ToDbValue(), FechaCreacion = DateTime.UtcNow, FechaExpiracion = DateTime.UtcNow.AddDays(1), Usado = false });
            await context.SaveChangesAsync();

            RefreshTokenRepository repository = new RefreshTokenRepository(context);
            TokenSeguridad? token = await repository.ObtenerPorTokenAsync("refresh");

            Assert.NotNull(token);
            Assert.Equal(1, token!.Id);
        }

        [Fact]
        public async Task RevocarTodosLosTokensDelUsuarioAsync_Debe_Marcar_Usados()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.TokensSeguridad.AddRangeAsync(
                new TokenSeguridad { Id = 1, UsuarioId = 5, Token = "a", TipoToken = TipoTokenSeguridad.RefreshToken.ToDbValue(), FechaCreacion = DateTime.UtcNow, FechaExpiracion = DateTime.UtcNow.AddDays(1), Usado = false },
                new TokenSeguridad { Id = 2, UsuarioId = 5, Token = "b", TipoToken = TipoTokenSeguridad.RefreshToken.ToDbValue(), FechaCreacion = DateTime.UtcNow, FechaExpiracion = DateTime.UtcNow.AddDays(1), Usado = false });
            await context.SaveChangesAsync();

            RefreshTokenRepository repository = new RefreshTokenRepository(context);
            await repository.RevocarTodosLosTokensDelUsuarioAsync(5);

            List<TokenSeguridad> tokens = await context.TokensSeguridad.Where(item => item.UsuarioId == 5).ToListAsync();
            Assert.All(tokens, item => Assert.True(item.Usado));
        }

        [Fact]
        public async Task ObtenerUsuarioPorIdAsync_Debe_Devolver_Usuario()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());

            await context.Usuarios.AddAsync(new Usuario
            {
                Id = 99,
                Nombre = "Usuario Token",
                Email = "token@mail.com",
                Rol = global::Talleres360.Enum.RolesUsuario.ADMIN,
                Activo = false,
                Eliminado = false,
                FechaCreacion = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            RefreshTokenRepository repository = new RefreshTokenRepository(context);
            Usuario? usuario = await repository.ObtenerUsuarioPorIdAsync(99);

            Assert.NotNull(usuario);
            Assert.Equal("token@mail.com", usuario!.Email);
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
