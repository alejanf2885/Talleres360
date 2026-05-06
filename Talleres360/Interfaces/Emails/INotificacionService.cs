using Talleres360.Dtos.Responses;

namespace Talleres360.Interfaces.Emails
{
    public interface INotificacionService
    {
        Task<ServiceResult<bool>> EnviarBienvenidaAsync(Usuario usuario, string link);
    }
}
