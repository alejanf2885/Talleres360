using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Repositories.Seguridad;
using Talleres360.Models;

namespace Talleres360.Test.Repositories
{
    public class VerificacionRepositoryTests
    {
        [Fact]
        public async Task AddAsync_Y_GetByTokenAsync_Deben_Funcionar()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            VerificacionRepository repository = new VerificacionRepository(context);

            UsuarioVerificacion verificacion = new UsuarioVerificacion
            {
                UsuarioId = 5,
                Token = "token-123",
                Tipo = "EMAIL",
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(1)
            };

            await repository.AddAsync(verificacion);
            UsuarioVerificacion? encontrada = await repository.GetByTokenAsync("token-123");

            Assert.NotNull(encontrada);
            Assert.Equal(5, encontrada!.UsuarioId);
        }

        [Fact]
        public async Task DeleteAsync_Debe_Eliminar_Registro()
        {
            ApplicationDbContext context = CrearContexto(Guid.NewGuid().ToString());
            VerificacionRepository repository = new VerificacionRepository(context);

            UsuarioVerificacion verificacion = new UsuarioVerificacion
            {
                UsuarioId = 5,
                Token = "token-delete",
                Tipo = "EMAIL",
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(1)
            };

            await repository.AddAsync(verificacion);
            await repository.DeleteAsync(verificacion);

            UsuarioVerificacion? encontrada = await repository.GetByTokenAsync("token-delete");
            Assert.Null(encontrada);
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
