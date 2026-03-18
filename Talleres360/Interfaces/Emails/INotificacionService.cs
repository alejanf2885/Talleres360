using Talleres360.Dtos.Responses;
using Talleres360.Models;

namespace Talleres360.Interfaces.Emails
{
    public interface INotificacionService
    {
        Task<ServiceResult<bool>> EnviarBienvenidaAsync(Usuario usuario, string token);
    }
}
