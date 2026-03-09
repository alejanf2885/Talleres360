using Talleres360.Models;

namespace Talleres360.Interfaces.Usuarios
{
    public interface IIdentityService
    {
        Task<(bool Success, string Message)> RegistrarTallerNuevoAsync(
            string nombreTaller,
            string nombreAdmin,
            string email,
            string password);

        Task<Usuario?> ValidarLoginAsync(string email, string password);
    }
}