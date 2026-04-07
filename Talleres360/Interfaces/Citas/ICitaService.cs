using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;

namespace Talleres360.Interfaces.Citas
{
    public interface ICitaService
    {
        Task<PagedResponse<CitaDto>> ObtenerTodasAsync(int tallerId, PaginationParams paginacion, DateTime? fechaDesde = null, DateTime? fechaHasta = null, CitaEstado? estado = null, int? vehiculoId = null);
        Task<ServiceResult<CitaDto>> ObtenerPorIdAsync(int tallerId, int citaId);
        Task<ServiceResult<CitaDto>> CrearAsync(int tallerId, CrearCitaRequest request);
        Task<ServiceResult<CitaDto>> ActualizarAsync(int tallerId, int citaId, ActualizarCitaRequest request);
        Task<ServiceResult<bool>> EliminarAsync(int tallerId, int citaId);
        Task<ServiceResult<CitaTrabajoDto>> ConvertirATrabajoAsync(int tallerId, int citaId, int? usuarioId, ConvertirCitaTrabajoRequest request);
    }
}
