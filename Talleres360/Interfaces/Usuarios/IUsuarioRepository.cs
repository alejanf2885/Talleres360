using Talleres360.Models;

namespace Talleres360.Interfaces.Usuarios
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetByEmailAsync(string email);
        Task AddAsync(Usuario usuario);
        Task AddCredencialAsync(Credencial credencial);
        Task SaveChangesAsync();
        Task<bool> ExisteEmailAsync(string email);
        Task<Credencial?> GetCredencialLocalByUsuarioIdAsync(int usuarioId);
        Task ActualizarUltimoAccesoAsync(int usuarioId);
        Task<Usuario?> GetByIdAsync(int id);
    }
}
