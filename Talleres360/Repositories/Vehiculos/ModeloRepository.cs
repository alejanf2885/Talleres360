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
    }
}
