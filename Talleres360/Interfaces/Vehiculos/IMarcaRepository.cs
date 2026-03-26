using Talleres360.Dtos.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IMarcaRepository
    {

        Task<Marca> GetMarcaById(int id);
        Task<List<MarcaVehiculoDto>> ObtenerMarcasAsync(int tallerId);
        Task<bool> ExisteMarcaVisibleAsync(int tallerId, int marcaId);

        Task CrearMarcaAsync(Marca marca);

        Task<Marca> GetMarcaByNombreAsync(string nombre, int idTaller);
    }
}
