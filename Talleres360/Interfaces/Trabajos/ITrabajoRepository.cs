using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Interfaces.Trabajos
{
    public interface ITrabajoRepository : ITallerRecursoRepository
    {
        Task<PagedResponse<TrabajoDto>> ObtenerTodosPagedAsync(int tallerId, PaginationParams paginacion, TrabajoEstado? estado = null, int? vehiculoId = null, bool? datosIncompletos = null);
        Task<PagedResponse<TrabajoDto>> ObtenerPresupuestosPagedAsync(int tallerId, PaginationParams paginacion);
        Task<Trabajo?> ObtenerEntidadPorIdAsync(int trabajoId);
        Task<TrabajoDto?> ObtenerDetallePorIdAsync(int trabajoId);
        Task AddAsync(Trabajo trabajo);
        Task UpdateAsync(Trabajo trabajo);
        Task<string> GenerarNumeroDocumentoTrabajoAsync(int tallerId);
    }
}
