using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Repositories.Vehiculos
{


    public class VehiculoRepository : IVehiculoRepository
    {
        private readonly ApplicationDbContext _context;

        public VehiculoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            Vehiculo? vehiculo = await _context.Vehiculos.FindAsync(id);
            return vehiculo;
        }

        public async Task<Vehiculo?> GetByMatriculaAsync(string matricula)
        {
            Vehiculo? vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Matricula == matricula);
            return vehiculo;
        }

        public async Task AddAsync(Vehiculo vehiculo)
        {
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehiculo vehiculo)
        {
            _context.Vehiculos.Update(vehiculo);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string matricula)
        {
            bool exists = await _context.Vehiculos
                .AnyAsync(v => v.Matricula == matricula);
            return exists;
        }

     
        public async Task<PagedResponse<VehiculoDetalle>> GetAllDetalleByTallerAsync(
            int tallerId,
            int pageNumber,
            int pageSize,
            VehiculoFiltroDto? filtro = null)
        {
            IQueryable<VehiculoDetalle> query = _context.VehiculosDetalle
                .Where(v => v.TallerId == tallerId && !v.Eliminado);

            if (filtro != null)
            {
                if (!string.IsNullOrWhiteSpace(filtro.Matricula))
                {
                    string m = filtro.Matricula.ToLower();
                    query = query.Where(v => v.Matricula.ToLower().Contains(m));
                }

                if (filtro.MarcaId.HasValue)
                {
                    int marca = filtro.MarcaId.Value;
                    query = query.Where(v => v.MarcaId == marca);
                }

                if (filtro.ModeloId.HasValue)
                {
                    int modelo = filtro.ModeloId.Value;
                    query = query.Where(v => v.ModeloId == modelo);
                }

                if (filtro.TipoVehiculoId.HasValue)
                {
                    int tipo = filtro.TipoVehiculoId.Value;
                    query = query.Where(v => v.TipoVehiculoId == tipo);
                }

                if (filtro.Anio.HasValue)
                {
                    int anio = filtro.Anio.Value;
                    query = query.Where(v => v.Anio == anio);
                }
            }

            int totalCount = await query.CountAsync();

            List<VehiculoDetalle> data = await query
                .OrderByDescending(v => v.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            PagedResponse<VehiculoDetalle> response = new PagedResponse<VehiculoDetalle>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return response;
        }

        public async Task<VehiculoDetalle?> GetDetalleByIdAsync(int id)
        {
            VehiculoDetalle? detalle = await _context.VehiculosDetalle
                .FirstOrDefaultAsync(v => v.Id == id);
            return detalle;
        }

        public async Task<VehiculoDetalle?> GetDetalleByMatriculaAsync(string matricula)
        {
            VehiculoDetalle? detalle = await _context.VehiculosDetalle
                .FirstOrDefaultAsync(v => v.Matricula == matricula);
            return detalle;
        }
    }
}