using Talleres360.Dtos.NotasVehiculo;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Interfaces.NotasVehiculo
{
    public interface INotaVehiculoRepository : ITallerRecursoRepository
    {
        Task<List<NotaVehiculoDto>> ObtenerPorVehiculoAsync(int tallerId, int vehiculoId);
        Task<NotaVehiculo?> ObtenerEntidadPorIdAsync(int notaId);
        Task<NotaVehiculoDto?> ObtenerDetallePorIdAsync(int notaId);
        Task AddAsync(NotaVehiculo nota);
        Task UpdateAsync(NotaVehiculo nota);
    }
}
