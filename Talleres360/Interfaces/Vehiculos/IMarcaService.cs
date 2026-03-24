using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IMarcaService
    {
        Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId);
    }
}
