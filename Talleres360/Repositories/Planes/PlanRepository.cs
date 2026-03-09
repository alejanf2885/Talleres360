using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Interfaces.Planes;
using Talleres360.Models;

namespace Talleres360.Repositories.Planes
{
    public class PlanRepository : IPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public PlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Plan?> GetPlanPorNombreAsync(string nombre)
        {
            return await _context.Planes
                .FirstOrDefaultAsync(p => p.Nombre == nombre);
        }

        public async Task<Plan?> GetPlanPorIdAsync(int id)
        {
            return await _context.Planes.FindAsync(id);
        }

        public async Task<IEnumerable<Plan>> GetPlanesActivosAsync()
        {
            return await _context.Planes.ToListAsync();
        }
    }
}