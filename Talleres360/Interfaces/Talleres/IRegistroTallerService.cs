using Talleres360.Dtos;
using Talleres360.Dtos.Responses;

namespace Talleres360.Interfaces.Talleres
{
    public interface IRegistroTallerService
    {
        Task<ServiceResult<bool>> RegistrarNuevoClienteSaaSAsync(RegistroRequest request);
    }
}