using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IMarcaRepository : ITallerRecursoRepository
    {
        Task<List<MarcaVehiculoDto>> ObtenerMarcasAsync(int tallerId);
        Task<bool> ExisteMarcaVisibleAsync(int tallerId, int marcaId);

        Task<Marca?> GetMarcaByIdAsync(int id);
        Task<Marca?> GetMarcaVisibleByNombreAsync(int tallerId, string nombre);

        Task<bool> ExisteMarcaOficialAsync(string nombre);
        Task<bool> ExisteMarcaEnTallerAsync(string nombre, int tallerId);
        Task<bool> TieneDependenciasAsync(int marcaId);

        Task AddAsync(Marca marca);
        Task UpdateAsync(Marca marca);
        Task DeleteAsync(Marca marca);
    }
}