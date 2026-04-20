using System.Threading.Tasks;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IVehiculoRepository : ITallerRecursoRepository
    {

        Task<Vehiculo?> GetByIdAsync(int id, int tallerId);

        Task AddAsync(Vehiculo vehiculo);

        Task UpdateAsync(Vehiculo vehiculo);

        Task<bool> ExistsAsync(string matricula, int tallerId);



        Task<VehiculoDetalle?> GetDetalleByIdAsync(int id);

        Task<VehiculoDetalle?> GetDetalleByMatriculaAsync(string matricula);

        Task<PagedResponse<VehiculoDetalle>> GetAllDetalleByTallerAsync(
            int tallerId,
            int pageNumber,
            int pageSize,
            VehiculoFiltroDto? filtro = null);


    }
}