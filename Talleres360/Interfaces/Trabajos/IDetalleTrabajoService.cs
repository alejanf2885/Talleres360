using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;

namespace Talleres360.Interfaces.Trabajos
{
    public interface IDetalleTrabajoService
    {
        Task<ServiceResult<IEnumerable<DetalleTrabajoDto>>> ObtenerPorTrabajoAsync(int tallerId, int trabajoId);
        Task<ServiceResult<DetalleTrabajoDto>> AgregarAsync(int tallerId, int trabajoId, CrearDetalleTrabajoRequest request);
        Task<ServiceResult<bool>> EliminarAsync(int tallerId, int trabajoId, int detalleId);
    }
}
