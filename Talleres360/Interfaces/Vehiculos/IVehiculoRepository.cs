using System.Threading.Tasks;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IVehiculoRepository
    {
    
        Task<Vehiculo?> GetByIdAsync(int id);
        Task<Vehiculo?> GetByMatriculaAsync(string matricula);
        Task AddAsync(Vehiculo vehiculo);
        Task UpdateAsync(Vehiculo vehiculo);
        Task<bool> ExistsAsync(string matricula);

        Task<PagedResponse<VehiculoDetalle>> GetAllDetalleByTallerAsync(
            int tallerId,
            int pageNumber,
            int pageSize,
            VehiculoFiltroDto? filtro = null);

        Task<VehiculoDetalle?> GetDetalleByIdAsync(int id);
        Task<VehiculoDetalle?> GetDetalleByMatriculaAsync(string matricula);
    }
}