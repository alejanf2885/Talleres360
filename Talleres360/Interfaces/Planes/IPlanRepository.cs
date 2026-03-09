using Talleres360.Models;

namespace Talleres360.Interfaces.Planes
{
    public interface IPlanRepository
    {
        Task<Plan?> GetPlanPorNombreAsync(string nombre);
        Task<Plan?> GetPlanPorIdAsync(int id);
        Task<IEnumerable<Plan>> GetPlanesActivosAsync();
    }
}
