using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IMarcaService
    {
        Task<ServiceResult<MarcaVehiculoDto>> GetByIdAsync(int id);
        Task<ServiceResult<MarcaVehiculoDto>> GetByNombreAsync(int tallerId, string nombre);
        Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId);
        Task<ServiceResult<MarcaVehiculoDto>> RegistrarMarcaAsync(int tallerId, string nombre, bool esOficial);
        Task<ServiceResult<bool>> EliminarMarcaAsync(int tallerId, int marcaId);
    }
}