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
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        public async Task<List<MarcaVehiculoDto>> ObtenerMarcasAsync(int tallerId)
        {
            return await _context.Marcas
                .AsNoTracking()
                .Where(marca =>  marca.TallerId == tallerId || marca.EsOficial)
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
                .FirstOrDefaultAsync(marca => marca.Id == id);
        }

        public async Task<Marca?> GetMarcaVisibleByNombreAsync(int tallerId, string nombre)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            return await _context.Marcas
                .AsNoTracking()
                .FirstOrDefaultAsync(marca =>
                    marca.Nombre.ToUpper() == nombreNormalizado &&
                    (marca.EsOficial || marca.TallerId == tallerId));
        }

        public async Task<bool> ExisteMarcaOficialAsync(string nombre)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            return await _context.Marcas
                .AsNoTracking()
                .AnyAsync(marca => marca.Nombre.ToUpper() == nombreNormalizado && marca.EsOficial);
        }

        public async Task<bool> ExisteMarcaEnTallerAsync(string nombre, int tallerId)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            return await _context.Marcas
                .AsNoTracking()
                .AnyAsync(marca =>
                    marca.Nombre.ToUpper() == nombreNormalizado &&
                    marca.TallerId == tallerId &&
                    !marca.EsOficial);
        }

        public async Task<bool> TieneDependenciasAsync(int marcaId)
        {
            bool tieneVehiculos = await _context.Vehiculos
                .AnyAsync(v => v.MarcaId == marcaId);

            if (tieneVehiculos)
            {
                return true;
            }

            bool tieneModelos = await _context.Modelos
                .AnyAsync(m => m.MarcaId == marcaId);

            return tieneModelos;
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

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            return await _context.Marcas
                .AsNoTracking()
                .AnyAsync(m => m.Id == id && m.TallerId == tallerId && !m.EsOficial);
        }
    }
}