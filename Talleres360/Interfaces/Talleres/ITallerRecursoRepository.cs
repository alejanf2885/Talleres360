

namespace Talleres360.Interfaces.Talleres
{
    public interface ITallerRecursoRepository
    {
        Task<bool> PerteneceATallerAsync(int id, int tallerId);
    }
}