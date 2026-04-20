
namespace Talleres360.Interfaces.Seguridad
{
    public interface IVerificacionRepository
    {
        Task AddAsync(UsuarioVerificacion verificacion);
        Task<UsuarioVerificacion?> GetByTokenAsync(string token);
        Task DeleteAsync(UsuarioVerificacion verificacion);
        Task<List<UsuarioVerificacion>> GetByUsuarioIdAsync(int usuarioId);
        Task LimpiarTokensExpiradosDelUsuarioAsync(int usuarioId);
        Task LimpiarTodosLosTokensDelUsuarioAsync(int usuarioId);
    }
}
