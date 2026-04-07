using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Enums;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Interfaces.Citas
{
    public interface ICitaRepository : ITallerRecursoRepository
    {
        Task<PagedResponse<CitaDto>> ObtenerTodasPagedAsync(int tallerId, PaginationParams paginacion, DateTime? fechaDesde = null, DateTime? fechaHasta = null, CitaEstado? estado = null, int? vehiculoId = null);
        Task<Cita?> ObtenerEntidadPorIdAsync(int citaId);
        Task<CitaDto?> ObtenerDetallePorIdAsync(int citaId);
        Task AddAsync(Cita cita);
        Task UpdateAsync(Cita cita);
    }
}
