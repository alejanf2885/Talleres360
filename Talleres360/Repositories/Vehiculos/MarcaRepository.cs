using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Repositories.Vehiculos
{
    public class MarcaRepository : IMarcaRepository
    {
        private readonly ApplicationDbContext _context;

        public MarcaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MarcaVehiculoDto>> ObtenerMarcasAsync(int tallerId)
        {
            List<MarcaVehiculoDto> marcas = await _context.Marcas
                .AsNoTracking()
                .Where(marca => marca.EsOficial || marca.TallerId == tallerId)
                .Select(marca => new MarcaVehiculoDto
                {
                    Id        = marca.Id,
                    Nombre    = marca.Nombre,
                    EsOficial = marca.EsOficial
                })
                .OrderBy(marca => marca.Nombre)
                .ToListAsync();

            return marcas;
        }

        public async Task<bool> ExisteMarcaVisibleAsync(int tallerId, int marcaId)
        {
            return await _context.Marcas
                .AsNoTracking()
                .AnyAsync(marca => marca.Id == marcaId && (marca.EsOficial || marca.TallerId == tallerId));
        }
    }
}
