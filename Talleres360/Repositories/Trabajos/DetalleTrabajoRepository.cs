using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Trabajos;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Models;

namespace Talleres360.Repositories.Trabajos
{
    public class DetalleTrabajoRepository : IDetalleTrabajoRepository
    {
        private readonly ApplicationDbContext _context;

        public DetalleTrabajoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DetalleTrabajoDto>> ObtenerPorTrabajoAsync(int trabajoId) =>
            await _context.DetallesTrabajo
                .AsNoTracking()
                .Where(d => d.TrabajoId == trabajoId && !d.Eliminado)
                .OrderBy(d => d.Id)
                .Select(d => new DetalleTrabajoDto
                {
                    Id                 = d.Id,
                    TrabajoId          = d.TrabajoId,
                    ProductoId         = d.ProductoId,
                    Concepto           = d.Concepto,
                    Cantidad           = d.Cantidad,
                    PrecioUnitario     = d.PrecioUnitario,
                    DescuentoPorcentaje = d.DescuentoPorcentaje,
                    ImpuestoPorcentaje = d.ImpuestoPorcentaje,
                    EsManoObra         = d.EsManoObra,
                    EstadoMaterial     = d.EstadoMaterial,
                    TarifaHoraId       = d.TarifaHoraId,
                    PrecioHoraAplicado = d.PrecioHoraAplicado,
                    HorasAplicadas     = d.HorasAplicadas
                })
                .ToListAsync();

        public async Task<DetalleTrabajo?> GetByIdAsync(int id, int trabajoId) =>
            await _context.DetallesTrabajo
                .FirstOrDefaultAsync(d => d.Id == id && d.TrabajoId == trabajoId && !d.Eliminado);

        public async Task AddAsync(DetalleTrabajo detalle) =>
            await _context.DetallesTrabajo.AddAsync(detalle);

        public Task UpdateAsync(DetalleTrabajo detalle)
        {
            _context.DetallesTrabajo.Update(detalle);
            return Task.CompletedTask;
        }

        public async Task<bool> PerteneceATrabajoAsync(int id, int trabajoId) =>
            await _context.DetallesTrabajo
                .AsNoTracking()
                .AnyAsync(d => d.Id == id && d.TrabajoId == trabajoId && !d.Eliminado);
    }
}
