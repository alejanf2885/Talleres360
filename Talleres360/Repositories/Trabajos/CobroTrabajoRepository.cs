using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Models;

namespace Talleres360.Repositories.Trabajos
{
    public class CobroTrabajoRepository : ICobroTrabajoRepository
    {
        private readonly ApplicationDbContext _context;

        public CobroTrabajoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<CobroTrabajoDto>> ObtenerTodosPorTrabajoPagedAsync(int trabajoId, PaginationParams paginacion)
        {
            IQueryable<CobroTrabajo> query = _context.CobrosTrabajo
                .AsNoTracking()
                .Where(cobro => cobro.TrabajoId == trabajoId && !cobro.Eliminado);

            int totalCount = await query.CountAsync();

            List<CobroTrabajoDto> data = await query
                .OrderByDescending(cobro => cobro.FechaCobro)
                .ThenByDescending(cobro => cobro.Id)
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .Select(cobro => new CobroTrabajoDto
                {
                    Id            = cobro.Id,
                    TrabajoId     = cobro.TrabajoId,
                    Importe       = cobro.Importe,
                    MetodoPago    = cobro.MetodoPago,
                    Referencia    = cobro.Referencia,
                    Notas         = cobro.Notas,
                    FechaCobro    = cobro.FechaCobro
                })
                .ToListAsync();

            return new PagedResponse<CobroTrabajoDto>
            {
                Data       = data,
                PageNumber = paginacion.PageNumber,
                PageSize   = paginacion.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<CobroTrabajo?> ObtenerEntidadPorIdAsync(int cobroId)
        {
            return await _context.CobrosTrabajo.FindAsync(cobroId);
        }

        public async Task<decimal> ObtenerTotalCobradoAsync(int trabajoId)
        {
            return await _context.CobrosTrabajo
                .AsNoTracking()
                .Where(cobro => cobro.TrabajoId == trabajoId && !cobro.Eliminado)
                .SumAsync(cobro => cobro.Importe);
        }

        public async Task AddAsync(CobroTrabajo cobro)
        {
            await _context.CobrosTrabajo.AddAsync(cobro);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CobroTrabajo cobro)
        {
            _context.CobrosTrabajo.Update(cobro);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            return await _context.CobrosTrabajo
                .AsNoTracking()
                .AnyAsync(cobro => cobro.Id == id && cobro.TallerId == tallerId && !cobro.Eliminado);
        }
    }
}
