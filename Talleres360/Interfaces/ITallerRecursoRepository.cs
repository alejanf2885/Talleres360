using System.Threading.Tasks;

namespace Talleres360.Interfaces
{
    public interface ITallerRecursoRepository
    {
        Task<bool> PerteneceATallerAsync(int id, int tallerId);
    }
}