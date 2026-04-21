using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Repositories.Trabajos
{
    public class TrabajoRepository : ITrabajoRepository
    {
        private readonly ApplicationDbContext _context;

        public TrabajoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<TrabajoDto>> ObtenerTodosPagedAsync(int tallerId, PaginationParams paginacion, TrabajoEstado? estado = null, int? vehiculoId = null, bool? datosIncompletos = null)
        {
            IQueryable<Trabajo> query = _context.Trabajos
                .AsNoTracking()
                .Where(trabajo => trabajo.TallerId == tallerId && !trabajo.Eliminado);

            // Excluir estados de presupuesto del listado de trabajos
            query = query.Where(trabajo => !TrabajoEstadoExtensions.EstadosPresupuesto.Contains(trabajo.Estado));

            if (estado.HasValue)
            {
                query = query.Where(trabajo => trabajo.Estado == estado.Value);
            }

            if (vehiculoId.HasValue)
            {
                query = query.Where(trabajo => trabajo.VehiculoId == vehiculoId.Value);
            }

            if (datosIncompletos.HasValue)
            {
                query = query.Where(trabajo => trabajo.DatosIncompletos == datosIncompletos.Value);
            }

            int totalCount = await query.CountAsync();

            List<TrabajoDto> data = await query
                .OrderByDescending(trabajo => trabajo.Id)
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .Select(trabajo => new TrabajoDto
                {
                    Id                     = trabajo.Id,
                    VehiculoId             = trabajo.VehiculoId,
                    MecanicoAsignadoId     = trabajo.MecanicoAsignadoId,
                    CitaId                 = trabajo.CitaId,
                    NumeroDocumento        = trabajo.NumeroDocumento,
                    TituloMantenimiento    = trabajo.TituloMantenimiento,
                    TrabajoRealizado       = trabajo.TrabajoRealizado,
                    KmEntrada              = trabajo.KmEntrada,
                    Estado                 = trabajo.Estado,
                    EstadoPago             = trabajo.EstadoPago,
                    Subtotal               = trabajo.Subtotal,
                    ImporteImpuestos       = trabajo.ImporteImpuestos,
                    Total                  = trabajo.Total,
                    CreadoPorId            = trabajo.CreadoPorId,
                    FechaCreacion          = trabajo.FechaCreacion,
                    ModificadoPorId        = trabajo.ModificadoPorId,
                    FechaUltimaModificacion = trabajo.FechaUltimaModificacion,
                    DatosIncompletos       = trabajo.DatosIncompletos,
                    FechaCierre            = trabajo.FechaCierre,
                    FechaEntregaEstimada   = trabajo.FechaEntregaEstimada,
                    KmSalida               = trabajo.KmSalida,
                    FotoEntradaUrl         = trabajo.FotoEntradaUrl,
                    FotoSalidaUrl          = trabajo.FotoSalidaUrl,
                    ObservacionesEntrega   = trabajo.ObservacionesEntrega
                })
                .ToListAsync();

            PagedResponse<TrabajoDto> respuesta = new PagedResponse<TrabajoDto>
            {
                Data = data,
                PageNumber = paginacion.PageNumber,
                PageSize = paginacion.PageSize,
                TotalCount = totalCount
            };

            return respuesta;
        }

        public async Task<PagedResponse<TrabajoDto>> ObtenerPresupuestosPagedAsync(int tallerId, PaginationParams paginacion)
        {
            IQueryable<Trabajo> query = _context.Trabajos
                .AsNoTracking()
                .Where(trabajo => trabajo.TallerId == tallerId
                    && !trabajo.Eliminado
                    && TrabajoEstadoExtensions.EstadosPresupuesto.Contains(trabajo.Estado));

            int totalCount = await query.CountAsync();

            List<TrabajoDto> data = await query
                .OrderByDescending(trabajo => trabajo.Id)
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .Select(trabajo => new TrabajoDto
                {
                    Id                     = trabajo.Id,
                    VehiculoId             = trabajo.VehiculoId,
                    MecanicoAsignadoId     = trabajo.MecanicoAsignadoId,
                    CitaId                 = trabajo.CitaId,
                    NumeroDocumento        = trabajo.NumeroDocumento,
                    TituloMantenimiento    = trabajo.TituloMantenimiento,
                    TrabajoRealizado       = trabajo.TrabajoRealizado,
                    KmEntrada              = trabajo.KmEntrada,
                    Estado                 = trabajo.Estado,
                    EstadoPago             = trabajo.EstadoPago,
                    Subtotal               = trabajo.Subtotal,
                    ImporteImpuestos       = trabajo.ImporteImpuestos,
                    Total                  = trabajo.Total,
                    CreadoPorId            = trabajo.CreadoPorId,
                    FechaCreacion          = trabajo.FechaCreacion,
                    ModificadoPorId        = trabajo.ModificadoPorId,
                    FechaUltimaModificacion = trabajo.FechaUltimaModificacion,
                    DatosIncompletos       = trabajo.DatosIncompletos,
                    FechaCierre            = trabajo.FechaCierre,
                    FechaEntregaEstimada   = trabajo.FechaEntregaEstimada,
                    KmSalida               = trabajo.KmSalida,
                    FotoEntradaUrl         = trabajo.FotoEntradaUrl,
                    FotoSalidaUrl          = trabajo.FotoSalidaUrl,
                    ObservacionesEntrega   = trabajo.ObservacionesEntrega
                })
                .ToListAsync();

            PagedResponse<TrabajoDto> respuesta = new PagedResponse<TrabajoDto>
            {
                Data = data,
                PageNumber = paginacion.PageNumber,
                PageSize = paginacion.PageSize,
                TotalCount = totalCount
            };

            return respuesta;
        }

        public async Task<Trabajo?> ObtenerEntidadPorIdAsync(int trabajoId)
        {
            Trabajo? trabajo = await _context.Trabajos.FindAsync(trabajoId);
            return trabajo;
        }

        public async Task<TrabajoDto?> ObtenerDetallePorIdAsync(int trabajoId)
        {
            TrabajoDto? trabajo = await _context.Trabajos
                .AsNoTracking()
                .Where(t => t.Id == trabajoId && !t.Eliminado)
                .Select(t => new TrabajoDto
                {
                    Id                     = t.Id,
                    VehiculoId             = t.VehiculoId,
                    MecanicoAsignadoId     = t.MecanicoAsignadoId,
                    CitaId                 = t.CitaId,
                    NumeroDocumento        = t.NumeroDocumento,
                    TituloMantenimiento    = t.TituloMantenimiento,
                    TrabajoRealizado       = t.TrabajoRealizado,
                    KmEntrada              = t.KmEntrada,
                    Estado                 = t.Estado,
                    EstadoPago             = t.EstadoPago,
                    Subtotal               = t.Subtotal,
                    ImporteImpuestos       = t.ImporteImpuestos,
                    Total                  = t.Total,
                    CreadoPorId            = t.CreadoPorId,
                    FechaCreacion          = t.FechaCreacion,
                    ModificadoPorId        = t.ModificadoPorId,
                    FechaUltimaModificacion = t.FechaUltimaModificacion,
                    DatosIncompletos       = t.DatosIncompletos,
                    FechaCierre            = t.FechaCierre,
                    FechaEntregaEstimada   = t.FechaEntregaEstimada,
                    KmSalida               = t.KmSalida,
                    FotoEntradaUrl         = t.FotoEntradaUrl,
                    FotoSalidaUrl          = t.FotoSalidaUrl,
                    ObservacionesEntrega   = t.ObservacionesEntrega
                })
                .FirstOrDefaultAsync();

            return trabajo;
        }

        public async Task AddAsync(Trabajo trabajo)
        {
            await _context.Trabajos.AddAsync(trabajo);
            await _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Trabajo trabajo)
        {
            _context.Trabajos.Update(trabajo);
            return Task.CompletedTask;
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            bool pertenece = await _context.Trabajos
                .AsNoTracking()
                .AnyAsync(trabajo => trabajo.Id == id && trabajo.TallerId == tallerId && !trabajo.Eliminado);

            return pertenece;
        }

        public async Task<string> GenerarNumeroDocumentoTrabajoAsync(int tallerId)
        {
            // Secuencia propia para trabajos/presupuestos (NO usa sp_SiguienteNumeroDocumento)
            int ultimoNumero = await _context.Trabajos
                .AsNoTracking()
                .Where(t => t.TallerId == tallerId)
                .OrderByDescending(t => t.Id)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            string numero = $"TRAB-{DateTime.UtcNow.Year}-{(ultimoNumero + 1):D6}";
            return numero;
        }
    }
}
