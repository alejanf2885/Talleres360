using Talleres360.Dtos.Trabajos;
using Talleres360.Models;

namespace Talleres360.Interfaces.Trabajos
{
    public interface IDetalleTrabajoRepository
    {
        Task<IEnumerable<DetalleTrabajoDto>> ObtenerPorTrabajoAsync(int trabajoId);
        Task<DetalleTrabajo?> GetByIdAsync(int id, int trabajoId);
        Task AddAsync(DetalleTrabajo detalle);
        Task UpdateAsync(DetalleTrabajo detalle);
        Task<bool> PerteneceATrabajoAsync(int id, int trabajoId);
    }
}
