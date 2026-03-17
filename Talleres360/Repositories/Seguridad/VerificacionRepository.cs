using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;

namespace Talleres360.Repositories.Seguridad
{
    public class VerificacionRepository : IVerificacionRepository
    {
        private readonly ApplicationDbContext _context;

        public VerificacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UsuarioVerificacion verificacion)
        {
            _context.UsuarioVerificaciones.Add(verificacion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UsuarioVerificacion verificacion)
        {
            _context.UsuarioVerificaciones.Remove(verificacion);
            await _context.SaveChangesAsync();
        }

        public async Task<UsuarioVerificacion?> GetByTokenAsync(string token)
        {
            return await _context.UsuarioVerificaciones
                .FirstOrDefaultAsync(v => v.Token == token);
        }
    }
}
