using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Repositories.Vehiculos
{
    public class ModeloRepository : IModeloRepository
    {
        private readonly ApplicationDbContext _context;

        public ModeloRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ModeloVehiculoDto>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId)
        {
            List<ModeloVehiculoDto> modelos = await _context.Modelos
                .AsNoTracking()
                .Where(modelo => modelo.MarcaId == marcaId && (modelo.EsOficial || modelo.TallerId == tallerId))
                .Select(modelo => new ModeloVehiculoDto
                {
                    Id             = modelo.Id,
                    MarcaId        = modelo.MarcaId,
                    VehiculoTipoId = modelo.VehiculoTipoId,
                    Nombre         = modelo.Nombre,
                    EsOficial      = modelo.EsOficial
                })
                .OrderBy(modelo => modelo.Nombre)
                .ToListAsync();

            return modelos;
        }

        public async Task<Modelo?> GetByIdAsync(int modeloId, int tallerId)
        {
            return await _context.Modelos
                .AsNoTracking()
                .FirstOrDefaultAsync(modelo => modelo.Id == modeloId && (modelo.EsOficial || modelo.TallerId == tallerId));
        }

        public async Task<bool> ExisteModeloOficialAsync(int marcaId, string nombre)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            return await _context.Modelos
                .AsNoTracking()
                .AnyAsync(modelo =>
                    modelo.MarcaId == marcaId &&
                    modelo.Nombre.ToUpper() == nombreNormalizado &&
                    modelo.EsOficial);
        }

        public async Task<bool> ExisteModeloEnTallerAsync(int marcaId, string nombre, int tallerId)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            return await _context.Modelos
                .AsNoTracking()
                .AnyAsync(modelo =>
                    modelo.MarcaId == marcaId &&
                    modelo.Nombre.ToUpper() == nombreNormalizado &&
                    modelo.TallerId == tallerId &&
                    !modelo.EsOficial);
        }

        public async Task AddAsync(Modelo modelo)
        {
            await _context.Modelos.AddAsync(modelo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Modelo modelo)
        {
            _context.Modelos.Update(modelo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Modelo modelo)
        {
            _context.Modelos.Remove(modelo);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TieneDependenciasAsync(int modeloId)
        {
            return await _context.Vehiculos
                .AnyAsync(vehiculo => vehiculo.ModeloId == modeloId);
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            return await _context.Modelos
                .AsNoTracking()
                .AnyAsync(modelo => modelo.Id == id && modelo.TallerId == tallerId && !modelo.EsOficial);
        }
    }
}
