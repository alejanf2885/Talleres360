using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Enums;
using Talleres360.Interfaces.Citas;
using Talleres360.Models;

namespace Talleres360.Repositories.Citas
{
    public class CitaRepository : ICitaRepository
    {
        private readonly ApplicationDbContext _context;

        public CitaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<CitaDto>> ObtenerTodasPagedAsync(int tallerId, PaginationParams paginacion, DateTime? fechaDesde = null, DateTime? fechaHasta = null, CitaEstado? estado = null, int? vehiculoId = null)
        {
            IQueryable<Cita> query = _context.Citas
                .AsNoTracking()
                .Where(cita => cita.TallerId == tallerId && !cita.Eliminado);

            if (vehiculoId.HasValue)
            {
                query = query.Where(cita => cita.VehiculoId == vehiculoId.Value);
            }

            if (fechaDesde.HasValue)
            {
                DateTime desde = fechaDesde.Value.Date;
                query = query.Where(cita => cita.FechaCita >= desde);
            }

            if (fechaHasta.HasValue)
            {
                DateTime hasta = fechaHasta.Value.Date;
                query = query.Where(cita => cita.FechaCita <= hasta);
            }

            if (estado.HasValue)
            {
                query = query.Where(cita => cita.Estado == estado.Value);
            }

            int totalCount = await query.CountAsync();

            List<CitaDto> data = await query
                .OrderBy(cita => cita.FechaCita)
                .ThenBy(cita => cita.Id)
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .Select(cita => new CitaDto
                {
                    Id = cita.Id,
                    VehiculoId = cita.VehiculoId,
                    NombreClienteTemp = cita.NombreClienteTemp,
                    FechaCita = cita.FechaCita,
                    HoraAproximada = cita.HoraAproximada,
                    Descripcion = cita.Descripcion,
                    Estado = cita.Estado
                })
                .ToListAsync();

            PagedResponse<CitaDto> respuesta = new PagedResponse<CitaDto>
            {
                Data = data,
                PageNumber = paginacion.PageNumber,
                PageSize = paginacion.PageSize,
                TotalCount = totalCount
            };

            return respuesta;
        }

        public async Task<Cita?> ObtenerEntidadPorIdAsync(int citaId)
        {
            Cita? cita = await _context.Citas.FindAsync(citaId);
            return cita;
        }

        public async Task<CitaDto?> ObtenerDetallePorIdAsync(int citaId)
        {
            CitaDto? cita = await _context.Citas
                .AsNoTracking()
                .Where(c => c.Id == citaId && !c.Eliminado)
                .Select(c => new CitaDto
                {
                    Id = c.Id,
                    VehiculoId = c.VehiculoId,
                    NombreClienteTemp = c.NombreClienteTemp,
                    FechaCita = c.FechaCita,
                    HoraAproximada = c.HoraAproximada,
                    Descripcion = c.Descripcion,
                    Estado = c.Estado
                })
                .FirstOrDefaultAsync();

            return cita;
        }

        public async Task AddAsync(Cita cita)
        {
            await _context.Citas.AddAsync(cita);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cita cita)
        {
            _context.Citas.Update(cita);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            bool pertenece = await _context.Citas
                .AsNoTracking()
                .AnyAsync(cita => cita.Id == id && cita.TallerId == tallerId && !cita.Eliminado);

            return pertenece;
        }
    }
}
