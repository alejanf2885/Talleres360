using Talleres360.Models;

namespace Talleres360.Interfaces.Seguridad
{
    public interface IRefreshTokenRepository
    {
        Task<TokenSeguridad?> ObtenerPorTokenAsync(string token);
        Task<Usuario?> ObtenerUsuarioPorIdAsync(int usuarioId);
        Task AgregarAsync(TokenSeguridad token);
        Task ActualizarAsync(TokenSeguridad token);
        Task RevocarTodosLosTokensDelUsuarioAsync(int usuarioId);
    }
}