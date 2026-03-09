using Talleres360.Models;

namespace Talleres360.Interfaces.Talleres
{
    public interface ITallerRepository
    {
        Task AddAsync(Taller taller);
        Task<Taller?> GetByIdAsync(int id);
        Task UpdateAsync(Taller taller); 
    }
}
