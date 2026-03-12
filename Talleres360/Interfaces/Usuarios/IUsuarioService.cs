using Talleres360.Models;

namespace Talleres360.Interfaces.Usuarios
{
    public interface IUsuarioService
    {
        Task<Usuario> GetByEmailAsync(string email);
        Task<Usuario?> GetByIdAsync(int id);
        Task ActualizarUltimoAccesoAsync(int usuarioId);
        Task<(bool Success, string Message, Usuario? Usuario)> CrearUsuarioAdminAsync(int tallerId, string nombre, string email, string password);
    }
}

