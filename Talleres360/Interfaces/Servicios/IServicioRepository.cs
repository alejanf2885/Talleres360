using Talleres360.Dtos;
using Talleres360.Dtos.Servicios;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Interfaces.Servicios
{
    public interface IServicioRepository : ITallerRecursoRepository
    {
        Task<PagedResponse<ServicioDto>> ObtenerTodosPagedAsync(int tallerId, PaginationParams paginacion, string? buscar = null, bool? activo = null);
        Task<Servicio?> ObtenerEntidadPorIdAsync(int servicioId);
        Task<ServicioDto?> ObtenerDetallePorIdAsync(int servicioId);
        Task<bool> ExisteNombreAsync(int tallerId, string nombre, int? servicioExcluirId = null);
        Task AddAsync(Servicio servicio);
        Task UpdateAsync(Servicio servicio);
    }
}
