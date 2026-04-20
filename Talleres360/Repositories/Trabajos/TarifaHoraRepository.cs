using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Trabajos;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Repositories.Trabajos
{
    public class TarifaHoraRepository : ITarifaHoraRepository
    {
        private readonly ApplicationDbContext _context;

        public TarifaHoraRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TarifaHora?> ObtenerActivaAsync(int tallerId) =>
            await _context.TarifasHora
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TallerId == tallerId && t.Activa);

        public async Task<IEnumerable<TarifaHoraDto>> ObtenerHistorialAsync(int tallerId) =>
            await _context.TarifasHora
                .AsNoTracking()
                .Where(t => t.TallerId == tallerId)
                .OrderByDescending(t => t.FechaVigencia)
                .ThenByDescending(t => t.Id)
                .Select(t => new TarifaHoraDto
                {
                    Id            = t.Id,
                    TallerId      = t.TallerId,
                    PrecioHora    = t.PrecioHora,
                    Descripcion   = t.Descripcion,
                    FechaVigencia = t.FechaVigencia,
                    Activa        = t.Activa,
                    CreadoPorId   = t.CreadoPorId,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync();

        public async Task<TarifaHora?> GetByIdAsync(int id, int tallerId) =>
            await _context.TarifasHora
                .FirstOrDefaultAsync(t => t.Id == id && t.TallerId == tallerId);

        public async Task AddAsync(TarifaHora tarifa)
        {
            // Desactivar tarifa activa anterior del mismo taller
            TarifaHora? anterior = await _context.TarifasHora
                .FirstOrDefaultAsync(t => t.TallerId == tarifa.TallerId && t.Activa);

            if (anterior != null)
                anterior.Activa = false;

            await _context.TarifasHora.AddAsync(tarifa);
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId) =>
            await _context.TarifasHora
                .AsNoTracking()
                .AnyAsync(t => t.Id == id && t.TallerId == tallerId);
    }
}
