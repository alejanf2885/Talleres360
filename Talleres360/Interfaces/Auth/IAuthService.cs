using Talleres360.Models;

namespace Talleres360.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<Usuario?> ValidarLoginAsync(string email, string password);
    }
}