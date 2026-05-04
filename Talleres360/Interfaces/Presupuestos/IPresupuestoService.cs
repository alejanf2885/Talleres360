using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;

namespace Talleres360.Interfaces.Presupuestos
{
    public interface IPresupuestoService
    {
        Task<PagedResponse<TrabajoDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion);
        Task<ServiceResult<TrabajoDto>> ObtenerPorIdAsync(int tallerId, int presupuestoId);
        Task<ServiceResult<TrabajoDto>> CrearAsync(int tallerId, int? usuarioId, CrearTrabajoRequest request);
        Task<ServiceResult<TrabajoDto>> ActualizarAsync(int tallerId, int presupuestoId, int? usuarioId, ActualizarTrabajoRequest request);
        Task<ServiceResult<bool>> EliminarAsync(int tallerId, int presupuestoId);
        Task<ServiceResult<TrabajoDto>> EnviarAsync(int tallerId, int presupuestoId);
        Task<ServiceResult<TrabajoDto>> AceptarAsync(int tallerId, int presupuestoId, string? firmaAceptacionUrl);
        Task<ServiceResult<TrabajoDto>> RechazarAsync(int tallerId, int presupuestoId, string motivoRechazo);
    }
}
