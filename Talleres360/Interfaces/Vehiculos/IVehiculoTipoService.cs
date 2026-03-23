using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IVehiculoTipoService
    {
        Task<ServiceResult<List<VehiculoTipoDto>>> ObtenerTiposVehiculoAsync();
    }
}
