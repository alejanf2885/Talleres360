using Talleres360.Dtos.Responses;

namespace Talleres360.Interfaces.Seguridad
{
    public interface IVerificacionService
    {
        Task<string> GenerarTokenRegistroAsync(int usuarioId);
        Task<ServiceResult<int>> ValidarYConsumirTokenAsync(string token);
        Task<string> GenerarLinkVerificacionAsync(int usuarioId);
    }
}
