using Talleres360.Dtos.NotasVehiculo;
using Talleres360.Dtos.Responses;

namespace Talleres360.Interfaces.NotasVehiculo
{
    public interface INotaVehiculoService
    {
        Task<ServiceResult<List<NotaVehiculoDto>>> ObtenerPorVehiculoAsync(int tallerId, int vehiculoId);
        Task<ServiceResult<NotaVehiculoDto>> ObtenerPorIdAsync(int tallerId, int notaId);
        Task<ServiceResult<NotaVehiculoDto>> CrearAsync(int tallerId, int vehiculoId, int? usuarioId, CrearNotaVehiculoRequest request);
        Task<ServiceResult<NotaVehiculoDto>> ActualizarAsync(int tallerId, int notaId, ActualizarNotaVehiculoRequest request);
        Task<ServiceResult<NotaVehiculoDto>> ResolverAsync(int tallerId, int notaId);
        Task<ServiceResult<bool>> EliminarAsync(int tallerId, int notaId);
    }
}
