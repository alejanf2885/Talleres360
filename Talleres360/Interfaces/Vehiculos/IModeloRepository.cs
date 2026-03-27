using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IModeloRepository : ITallerRecursoRepository
    {
        Task<List<ModeloVehiculoDto>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId);
        Task<Modelo?> GetByIdAsync(int modeloId);
        Task<bool> ExisteModeloOficialAsync(int marcaId, string nombre);
        Task<bool> ExisteModeloEnTallerAsync(int marcaId, string nombre, int tallerId);
        Task<bool> TieneDependenciasAsync(int modeloId);
        Task AddAsync(Modelo modelo);
        Task UpdateAsync(Modelo modelo);
        Task DeleteAsync(Modelo modelo);
    }
}
