using Talleres360.Models;

namespace Talleres360.Interfaces.Talleres
{
    public interface ITallerRepository
    {
        Task AddAsync(Taller taller);
        Task<Taller?> GetByIdAsync(int id);
        Task<Taller?> GetByCifAsync(string cif);
        Task UpdateAsync(Taller taller);
    }
}