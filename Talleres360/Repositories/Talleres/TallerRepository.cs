using Microsoft.EntityFrameworkCore;
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
            await _context.SaveChangesAsync();
        }

        public async Task<Taller?> GetByIdAsync(int id)
        {
            return await _context.Talleres.FindAsync(id);
        }

        public async Task<Taller?> GetByCifAsync(string cif)
        {
            return await _context.Talleres
                .FirstOrDefaultAsync(t => t.Cif == cif);
        }

        public async Task UpdateAsync(Taller taller)
        {
            _context.Talleres.Update(taller);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByCifAsync(string cif)
        {
            return await _context.Talleres.AnyAsync(t => t.Cif == cif);
        }

        public async Task<bool> IsPerfilConfiguradoAsync(int tallerId)
        {
            return await _context.Talleres
                .Where(t => t.Id == tallerId)
                .Select(t => t.PerfilConfigurado)
                .FirstOrDefaultAsync();
        }
    }
}