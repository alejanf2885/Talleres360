using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Seguridad;

namespace Talleres360.Interfaces.Seguridad
{
    public interface IRefreshTokenService
    {
        Task<string> CrearRefreshTokenAsync(int usuarioId);
        Task<ServiceResult<TokenResponseDto>> ValidarYRenovarAsync(string refreshToken);
        Task<ServiceResult<bool>> RevocarRefreshTokenAsync(string refreshToken);
        Task<ServiceResult<bool>> RevocarTodosLosTokensAsync(int usuarioId);
    }
}
