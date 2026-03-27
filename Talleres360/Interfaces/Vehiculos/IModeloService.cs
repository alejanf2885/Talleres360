using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Modelo;
using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IModeloService
    {
        Task<ServiceResult<List<ModeloVehiculoDto>>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId);
        Task<ServiceResult<ModeloVehiculoDto>> CrearModeloAsync(int tallerId, CrearModeloVehiculoDto crearModeloDto, bool esOficial);
        Task<ServiceResult<ModeloVehiculoDto>> ActualizarModeloAsync(int tallerId, int modeloId, ActualizarModeloVehiculoDto actualizarModeloDto);
        Task<ServiceResult<bool>> EliminarModeloAsync(int tallerId, int modeloId);
    }
}
