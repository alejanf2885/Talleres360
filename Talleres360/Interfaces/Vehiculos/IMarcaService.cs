using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IMarcaService
    {
        Task<ServiceResult<MarcaVehiculoDto>> GetByIdAsync(int id);
        Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId);
        Task<ServiceResult<MarcaVehiculoDto>> RegistrarMarcaAsync(Marca marca);

        Task<ServiceResult<MarcaVehiculoDto>> GetByNombreAsync(string nombre);
    }
}
