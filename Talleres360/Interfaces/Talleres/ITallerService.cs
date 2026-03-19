using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Talleres;
using Talleres360.Models;

namespace Talleres360.Interfaces.Talleres
{
    public interface ITallerService
    {
        Task<ServiceResult<Taller>> CrearTallerBaseAsync(string nombreNegocio, int planId);
        Task<ServiceResult<bool>> ConfigurarPerfilAsync(int tallerId, ConfigurarTallerRequest request);
        Task<ServiceResult<WorkshopDto>> ObtenerTallerPorIdAsync(int tallerId);
    }
}