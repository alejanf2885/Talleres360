using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Repositories.Vehiculos
{
    public class VehiculoTipoRepository : IVehiculoTipoRepository
    {
        private readonly ApplicationDbContext _context;

        public VehiculoTipoRepository(ApplicationDbContext context)
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
    }
}
