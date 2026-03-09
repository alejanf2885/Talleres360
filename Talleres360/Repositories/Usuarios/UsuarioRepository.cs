using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

namespace Talleres360.Repositories.Usuarios
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _context.Usuarios
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == email);
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
        }

        public async Task AddCredencialAsync(Credencial credencial)
        {
            await _context.Credenciales.AddAsync(credencial);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Credencial?> GetCredencialLocalByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Credenciales
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId
                                       && c.TipoInicioSesion == "LOCAL"
                                       && !c.Eliminado);
        }

        public async Task ActualizarUltimoAccesoAsync(int usuarioId)
        {
            Credencial? credencial = await _context.Credenciales
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId
                                       && c.TipoInicioSesion == "LOCAL"
                                       && !c.Eliminado);

            if (credencial != null)
            {
                credencial.FechaUltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
