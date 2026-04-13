using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Interfaces.Trabajos
{
    public interface ICobroTrabajoRepository : ITallerRecursoRepository
    {
        Task<PagedResponse<CobroTrabajoDto>> ObtenerTodosPorTrabajoPagedAsync(int trabajoId, PaginationParams paginacion);
        Task<CobroTrabajo?> ObtenerEntidadPorIdAsync(int cobroId);
        Task<decimal> ObtenerTotalCobradoAsync(int trabajoId);
        Task AddAsync(CobroTrabajo cobro);
        Task UpdateAsync(CobroTrabajo cobro);
    }
}
