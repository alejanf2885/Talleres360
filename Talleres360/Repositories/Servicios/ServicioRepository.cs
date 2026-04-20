using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Servicios;
using Talleres360.Interfaces.Servicios;

namespace Talleres360.Repositories.Servicios
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly ApplicationDbContext _context;

        public ServicioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<ServicioDto>> ObtenerTodosPagedAsync(int tallerId, PaginationParams paginacion, string? buscar = null, bool? activo = null)
        {
            IQueryable<Servicio> query = _context.Servicios
                .AsNoTracking()
                .Where(servicio => servicio.TallerId == tallerId && !servicio.Eliminado);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string criterio = buscar.Trim().ToLower();
                query = query.Where(servicio => servicio.Nombre.ToLower().Contains(criterio) || (servicio.Descripcion != null && servicio.Descripcion.ToLower().Contains(criterio)));
            }

            if (activo.HasValue)
            {
                query = query.Where(servicio => servicio.Activo == activo.Value);
            }

            int totalCount = await query.CountAsync();

            List<ServicioDto> data = await query
                .OrderBy(servicio => servicio.Nombre)
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .Select(servicio => new ServicioDto
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    PrecioBase = servicio.PrecioBase,
                    ImpuestoPorcentaje = servicio.ImpuestoPorcentaje,
                    Activo = servicio.Activo
                })
                .ToListAsync();

            PagedResponse<ServicioDto> respuesta = new PagedResponse<ServicioDto>
            {
                Data = data,
                PageNumber = paginacion.PageNumber,
                PageSize = paginacion.PageSize,
                TotalCount = totalCount
            };

            return respuesta;
        }

        public async Task<Servicio?> ObtenerEntidadPorIdAsync(int servicioId)
        {
            Servicio? servicio = await _context.Servicios.FindAsync(servicioId);
            return servicio;
        }

        public async Task<ServicioDto?> ObtenerDetallePorIdAsync(int servicioId)
        {
            ServicioDto? servicio = await _context.Servicios
                .AsNoTracking()
                .Where(item => item.Id == servicioId && !item.Eliminado)
                .Select(item => new ServicioDto
                {
                    Id = item.Id,
                    Nombre = item.Nombre,
                    Descripcion = item.Descripcion,
                    PrecioBase = item.PrecioBase,
                    ImpuestoPorcentaje = item.ImpuestoPorcentaje,
                    Activo = item.Activo
                })
                .FirstOrDefaultAsync();

            return servicio;
        }

        public async Task<bool> ExisteNombreAsync(int tallerId, string nombre, int? servicioExcluirId = null)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            bool existe = await _context.Servicios
                .AsNoTracking()
                .AnyAsync(servicio =>
                    servicio.TallerId == tallerId &&
                    !servicio.Eliminado &&
                    servicio.Nombre.ToUpper() == nombreNormalizado &&
                    (!servicioExcluirId.HasValue || servicio.Id != servicioExcluirId.Value));

            return existe;
        }

        public async Task AddAsync(Servicio servicio)
        {
            await _context.Servicios.AddAsync(servicio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Servicio servicio)
        {
            _context.Servicios.Update(servicio);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            bool pertenece = await _context.Servicios
                .AsNoTracking()
                .AnyAsync(servicio => servicio.Id == id && servicio.TallerId == tallerId && !servicio.Eliminado);

            return pertenece;
        }
    }
}
