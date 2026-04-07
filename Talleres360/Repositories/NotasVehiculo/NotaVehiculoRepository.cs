using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.NotasVehiculo;
using Talleres360.Interfaces.NotasVehiculo;
using Talleres360.Models;

namespace Talleres360.Repositories.NotasVehiculo
{
    public class NotaVehiculoRepository : INotaVehiculoRepository
    {
        private readonly ApplicationDbContext _context;

        public NotaVehiculoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotaVehiculoDto>> ObtenerPorVehiculoAsync(int tallerId, int vehiculoId)
        {
            List<NotaVehiculoDto> notas = await _context.NotasVehiculo
                .AsNoTracking()
                .Where(nota => nota.TallerId == tallerId && nota.VehiculoId == vehiculoId && !nota.Eliminado)
                .OrderByDescending(nota => nota.FechaCreacion)
                .Select(nota => new NotaVehiculoDto
                {
                    Id = nota.Id,
                    VehiculoId = nota.VehiculoId,
                    UsuarioId = nota.UsuarioId,
                    Texto = nota.Texto,
                    Tipo = nota.Tipo,
                    Resuelta = nota.Resuelta,
                    FechaCreacion = nota.FechaCreacion,
                    FechaResolucion = nota.FechaResolucion
                })
                .ToListAsync();

            return notas;
        }

        public async Task<NotaVehiculo?> ObtenerEntidadPorIdAsync(int notaId)
        {
            NotaVehiculo? nota = await _context.NotasVehiculo.FindAsync(notaId);
            return nota;
        }

        public async Task<NotaVehiculoDto?> ObtenerDetallePorIdAsync(int notaId)
        {
            NotaVehiculoDto? nota = await _context.NotasVehiculo
                .AsNoTracking()
                .Where(item => item.Id == notaId && !item.Eliminado)
                .Select(item => new NotaVehiculoDto
                {
                    Id = item.Id,
                    VehiculoId = item.VehiculoId,
                    UsuarioId = item.UsuarioId,
                    Texto = item.Texto,
                    Tipo = item.Tipo,
                    Resuelta = item.Resuelta,
                    FechaCreacion = item.FechaCreacion,
                    FechaResolucion = item.FechaResolucion
                })
                .FirstOrDefaultAsync();

            return nota;
        }

        public async Task AddAsync(NotaVehiculo nota)
        {
            await _context.NotasVehiculo.AddAsync(nota);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NotaVehiculo nota)
        {
            _context.NotasVehiculo.Update(nota);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            bool pertenece = await _context.NotasVehiculo
                .AsNoTracking()
                .AnyAsync(nota => nota.Id == id && nota.TallerId == tallerId && !nota.Eliminado);

            return pertenece;
        }
    }
}
