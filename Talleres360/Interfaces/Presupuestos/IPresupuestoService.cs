using Talleres360.Dtos;
using Talleres360.Dtos.Presupuestos;
using Talleres360.Dtos.Responses;

namespace Talleres360.Interfaces.Presupuestos
{
    public interface IPresupuestoService
    {
        Task<PagedResponse<PresupuestoDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion);
        Task<ServiceResult<PresupuestoDto>> ObtenerPorIdAsync(int tallerId, int presupuestoId);
        Task<ServiceResult<PresupuestoDto>> CrearAsync(int tallerId, CrearPresupuestoRequest request);
    }
}
