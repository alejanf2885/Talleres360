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

        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            return await _context.Vehiculos.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(string matricula)
        {
            return await _context.Vehiculos.AnyAsync(v => v.Matricula == matricula && !v.Eliminado);
        }

        public async Task<VehiculoDetalle?> GetDetalleByIdAsync(int id)
        {
            return await _context.VehiculosDetalle.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<VehiculoDetalle?> GetDetalleByMatriculaAsync(string matricula)
        {
            return await _context.VehiculosDetalle.FirstOrDefaultAsync(v => v.Matricula == matricula);
        }

        public async Task<PagedResponse<VehiculoDetalle>> GetAllDetalleByTallerAsync(
            int tallerId, int pageNumber, int pageSize, VehiculoFiltroDto? filtro = null)
        {
            IQueryable<VehiculoDetalle> query = _context.VehiculosDetalle
                .Where(v => v.TallerId == tallerId && !v.Eliminado);

            if (filtro != null)
            {
                if (!string.IsNullOrWhiteSpace(filtro.Matricula))
                {
                    string m = filtro.Matricula.ToUpper();
                    query = query.Where(v => v.Matricula.Contains(m));
                }
                if (filtro.MarcaId.HasValue) query = query.Where(v => v.MarcaId == filtro.MarcaId);
                if (filtro.ModeloId.HasValue) query = query.Where(v => v.ModeloId == filtro.ModeloId);
            }

            int totalCount = await query.CountAsync();
            List<VehiculoDetalle> data = await query
                .OrderByDescending(v => v.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<VehiculoDetalle>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            return await _context.Vehiculos.AnyAsync(v => v.Id == id && v.TallerId == tallerId);
        }
    }
}