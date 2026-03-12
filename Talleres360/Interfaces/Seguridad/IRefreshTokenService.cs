using Talleres360.Dtos.Seguridad;

namespace Talleres360.Interfaces.Seguridad
{
    public interface IRefreshTokenService
    {
        Task<string> CrearRefreshTokenAsync(int usuarioId);
        Task<TokenRefreshResult> ValidarYRenovarAsync(string refreshToken);
        Task RevocarRefreshTokenAsync(string refreshToken);
    }
}
