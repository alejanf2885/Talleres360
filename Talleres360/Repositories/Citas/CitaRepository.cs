using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Enums;
using Talleres360.Interfaces.Citas;

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
            IQueryable<Cita> citasBase = _context.Citas
                .AsNoTracking()
                .Where(cita => cita.TallerId == tallerId && !cita.Eliminado);

            if (vehiculoId.HasValue)
                citasBase = citasBase.Where(cita => cita.VehiculoId == vehiculoId.Value);

            if (fechaDesde.HasValue)
                citasBase = citasBase.Where(cita => cita.FechaCita >= fechaDesde.Value.Date);

            if (fechaHasta.HasValue)
                citasBase = citasBase.Where(cita => cita.FechaCita <= fechaHasta.Value.Date);

            if (estado.HasValue)
                citasBase = citasBase.Where(cita => cita.Estado == estado.Value);

            int totalCount = await citasBase.CountAsync();

            List<CitaDto> data = await (
                from cita in citasBase
                join vd in _context.VehiculosDetalle.AsNoTracking()
                    on cita.VehiculoId equals (int?)vd.Id into vdGroup
                from vd in vdGroup.DefaultIfEmpty()
                join c in _context.Clientes.AsNoTracking()
                    on vd.ClienteId equals c.Id into cGroup
                from c in cGroup.DefaultIfEmpty()
                orderby cita.FechaCita, cita.Id
                select new CitaDto
                {
                    Id                = cita.Id,
                    VehiculoId        = cita.VehiculoId,
                    VehiculoMatricula = vd == null ? null : vd.Matricula,
                    VehiculoMarca     = vd == null ? null : vd.MarcaNombre,
                    VehiculoModelo    = vd == null ? null : vd.ModeloNombre,
                    ClienteNombre     = c == null ? null : (c.Apellidos == null ? c.Nombre : c.Nombre + " " + c.Apellidos),
                    NombreClienteTemp = cita.NombreClienteTemp,
                    FechaCita         = cita.FechaCita,
                    HoraAproximada    = cita.HoraAproximada,
                    Descripcion       = cita.Descripcion,
                    Estado            = cita.Estado
                })
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .ToListAsync();

            return new PagedResponse<CitaDto>
            {
                Data       = data,
                PageNumber = paginacion.PageNumber,
                PageSize   = paginacion.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Cita?> ObtenerEntidadPorIdAsync(int citaId)
        {
            Cita? cita = await _context.Citas.FindAsync(citaId);
            return cita;
        }

        public async Task<CitaDto?> ObtenerDetallePorIdAsync(int citaId)
        {
            return await (
                from cita in _context.Citas.AsNoTracking()
                    .Where(c => c.Id == citaId && !c.Eliminado)
                join vd in _context.VehiculosDetalle.AsNoTracking()
                    on cita.VehiculoId equals (int?)vd.Id into vdGroup
                from vd in vdGroup.DefaultIfEmpty()
                join c in _context.Clientes.AsNoTracking()
                    on vd.ClienteId equals c.Id into cGroup
                from c in cGroup.DefaultIfEmpty()
                select new CitaDto
                {
                    Id                = cita.Id,
                    VehiculoId        = cita.VehiculoId,
                    VehiculoMatricula = vd == null ? null : vd.Matricula,
                    VehiculoMarca     = vd == null ? null : vd.MarcaNombre,
                    VehiculoModelo    = vd == null ? null : vd.ModeloNombre,
                    ClienteNombre     = c == null ? null : (c.Apellidos == null ? c.Nombre : c.Nombre + " " + c.Apellidos),
                    NombreClienteTemp = cita.NombreClienteTemp,
                    FechaCita         = cita.FechaCita,
                    HoraAproximada    = cita.HoraAproximada,
                    Descripcion       = cita.Descripcion,
                    Estado            = cita.Estado
                })
                .FirstOrDefaultAsync();
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
