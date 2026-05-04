using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;

namespace Talleres360.Interfaces.Trabajos
{
    public interface ITrabajoService
    {
        Task<PagedResponse<TrabajoDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion, TrabajoEstado? estado = null, int? vehiculoId = null, bool? datosIncompletos = null);
        Task<ServiceResult<TrabajoDto>> ObtenerPorIdAsync(int tallerId, int trabajoId);
        Task<ServiceResult<TrabajoDto>> CrearAsync(int tallerId, int? usuarioId, CrearTrabajoRequest request);
        Task<ServiceResult<TrabajoDto>> ActualizarAsync(int tallerId, int trabajoId, int? usuarioId, ActualizarTrabajoRequest request);
        Task<ServiceResult<bool>> EliminarAsync(int tallerId, int trabajoId);
        Task<ServiceResult<TrabajoDto>> FacturarAsync(int tallerId, int trabajoId);
    }
}
