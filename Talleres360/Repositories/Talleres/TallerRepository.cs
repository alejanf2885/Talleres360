using Talleres360.Data;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Repositories.Talleres
{
    public class TallerRepository : ITallerRepository
    {
        private readonly ApplicationDbContext _context;

        public TallerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Taller taller)
        {
            await _context.Talleres.AddAsync(taller);
        }

        public async Task<Taller?> GetByIdAsync(int id)
        {
            return await _context.Talleres.FindAsync(id);
        }
        public async Task UpdateAsync(Taller taller)
        {
            _context.Talleres.Update(taller);
            await _context.SaveChangesAsync();
        }
    }
}