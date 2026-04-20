using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Usuarios;

namespace Talleres360.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<ServiceResult<UsuarioLoginDto>> ValidarLoginAsync(string email, string password);
    }
}