using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

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
            return await _context.Marcas
                .AsNoTracking()
                .Where(marca => marca.EsOficial || marca.TallerId == tallerId)
                .Select(marca => new MarcaVehiculoDto
                {
                    Id = marca.Id,
                    Nombre = marca.Nombre,
                    EsOficial = marca.EsOficial
                })
                .OrderBy(marca => marca.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteMarcaVisibleAsync(int tallerId, int marcaId)
        {
            return await _context.Marcas
                .AsNoTracking()
                .AnyAsync(marca => marca.Id == marcaId && (marca.EsOficial || marca.TallerId == tallerId));
        }

        public async Task<Marca?> GetMarcaByIdAsync(int id)
        {
            return await _context.Marcas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Buscar marca visible para un taller (oficial O propia)
        public async Task<Marca?> GetMarcaVisibleByNombreAsync(int tallerId, string nombre)
        {
            return await _context.Marcas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Nombre.ToLower() == nombre.ToLower()
                                      && (m.EsOficial || m.TallerId == tallerId));
        }

        // Validar si existe marca oficial con ese nombre
        public async Task<bool> ExisteMarcaOficialAsync(string nombre)
        {
            return await _context.Marcas
                .AsNoTracking()
                .AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower() && m.EsOficial);
        }

        // Validar si existe marca del taller con ese nombre
        public async Task<bool> ExisteMarcaEnTallerAsync(string nombre, int tallerId)
        {
            return await _context.Marcas
                .AsNoTracking()
                .AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower()
                            && m.TallerId == tallerId
                            && !m.EsOficial);
        }

        public async Task AddAsync(Marca marca)
        {
            await _context.Marcas.AddAsync(marca);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Marca marca)
        {
            _context.Marcas.Update(marca);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Marca marca)
        {
            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();
        }
    }
}