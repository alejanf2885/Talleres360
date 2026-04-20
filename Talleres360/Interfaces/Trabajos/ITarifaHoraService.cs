using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;

namespace Talleres360.Interfaces.Trabajos
{
    public interface ITarifaHoraService
    {
        Task<ServiceResult<TarifaHoraDto>> CrearAsync(int tallerId, int usuarioId, CrearTarifaHoraRequest request);
        Task<ServiceResult<IEnumerable<TarifaHoraDto>>> ObtenerHistorialAsync(int tallerId);
        Task<ServiceResult<TarifaHoraDto?>> ObtenerActivaAsync(int tallerId);
    }
}
