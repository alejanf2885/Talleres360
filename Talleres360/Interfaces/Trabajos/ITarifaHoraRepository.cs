using Talleres360.Dtos.Trabajos;

namespace Talleres360.Interfaces.Trabajos
{
    public interface ITarifaHoraRepository
    {
        Task<TarifaHora?> ObtenerActivaAsync(int tallerId);
        Task<IEnumerable<TarifaHoraDto>> ObtenerHistorialAsync(int tallerId);
        Task<TarifaHora?> GetByIdAsync(int id, int tallerId);
        Task AddAsync(TarifaHora tarifa);
        Task<bool> PerteneceATallerAsync(int id, int tallerId);
    }
}
