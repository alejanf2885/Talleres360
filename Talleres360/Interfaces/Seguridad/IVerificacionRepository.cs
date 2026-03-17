using Talleres360.Models;

namespace Talleres360.Interfaces.Seguridad
{
    public interface IVerificacionRepository
    {
        Task AddAsync(UsuarioVerificacion verificacion);
        Task<UsuarioVerificacion?> GetByTokenAsync(string token);
        Task DeleteAsync(UsuarioVerificacion verificacion);
    }
}
