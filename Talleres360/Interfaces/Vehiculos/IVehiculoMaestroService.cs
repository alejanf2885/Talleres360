using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IVehiculoMaestroService
    {
        Task<ServiceResult<List<VehiculoTipoDto>>> ObtenerTiposVehiculoAsync();
        Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId);
        Task<ServiceResult<List<ModeloVehiculoDto>>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId);
    }
}
