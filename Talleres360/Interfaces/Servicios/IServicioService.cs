using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Servicios;

namespace Talleres360.Interfaces.Servicios
{
    public interface IServicioService
    {
        Task<PagedResponse<ServicioDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion, string? buscar = null, bool? activo = null);
        Task<ServiceResult<ServicioDto>> ObtenerPorIdAsync(int tallerId, int servicioId);
        Task<ServiceResult<ServicioDto>> CrearAsync(int tallerId, CrearServicioRequest request);
        Task<ServiceResult<ServicioDto>> ActualizarAsync(int tallerId, int servicioId, ActualizarServicioRequest request);
        Task<ServiceResult<bool>> EliminarAsync(int tallerId, int servicioId);
    }
}
