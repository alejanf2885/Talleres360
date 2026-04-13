using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;

namespace Talleres360.Interfaces.Trabajos
{
    public interface ICobroTrabajoService
    {
        Task<PagedResponse<CobroTrabajoDto>> ObtenerTodosPorTrabajoAsync(int tallerId, int trabajoId, PaginationParams paginacion);
        Task<ServiceResult<CobroTrabajoDto>> CrearAsync(int tallerId, int trabajoId, int? usuarioId, CrearCobroTrabajoRequest request);
        Task<ServiceResult<bool>> EliminarAsync(int tallerId, int cobroId);
    }
}
