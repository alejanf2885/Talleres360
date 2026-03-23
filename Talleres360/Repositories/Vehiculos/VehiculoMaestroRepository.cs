using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Repositories.Vehiculos
{
    public class VehiculoMaestroRepository : IVehiculoMaestroRepository
    {
        private readonly ApplicationDbContext _context;

        public VehiculoMaestroRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehiculoTipoDto>> ObtenerTiposVehiculoAsync()
        {
            List<VehiculoTipoDto> tipos = await _context.VehiculoTipos
                .AsNoTracking()
                .Select(tipo => new VehiculoTipoDto
                {
                    Id     = tipo.Id,
                    Nombre = tipo.Nombre
                })
                .OrderBy(tipo => tipo.Nombre)
                .ToListAsync();

            return tipos;
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
    }
}
