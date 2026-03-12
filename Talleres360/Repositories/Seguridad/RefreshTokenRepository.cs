using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;

namespace Talleres360.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TokenSeguridad?> ObtenerPorTokenAsync(string token)
        {
            // Quitamos el .Include() porque ya no tenemos la propiedad de navegación
            return await _context.TokensSeguridad
                .FirstOrDefaultAsync(t => t.Token == token && t.TipoToken == "REFRESH_TOKEN");
        }

        public async Task<Usuario?> ObtenerUsuarioPorIdAsync(int usuarioId)
        {
            return await _context.Usuarios.FindAsync(usuarioId);
        }

        public async Task AgregarAsync(TokenSeguridad token)
        {
            _context.TokensSeguridad.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(TokenSeguridad token)
        {
            _context.TokensSeguridad.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}