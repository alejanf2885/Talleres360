using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IModeloService
    {
        Task<ServiceResult<List<ModeloVehiculoDto>>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId);
    }
}
