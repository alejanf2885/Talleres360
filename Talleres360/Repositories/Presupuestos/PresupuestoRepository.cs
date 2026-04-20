using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Presupuestos;
using Talleres360.Enums;
using Talleres360.Interfaces.Presupuestos;

namespace Talleres360.Repositories.Presupuestos
{
    public class PresupuestoRepository : IPresupuestoRepository
    {
        private readonly ApplicationDbContext _context;

        public PresupuestoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<PresupuestoDto>> ObtenerTodosPagedAsync(int tallerId, PaginationParams paginacion)
        {
            IQueryable<Factura> query = _context.Facturas
                .AsNoTracking()
                .Where(factura => factura.TallerId == tallerId && factura.TipoDocumento == TipoDocumentoComercial.PRESUPUESTO);

            int totalCount = await query.CountAsync();

            List<PresupuestoDto> data = await query
                .OrderByDescending(factura => factura.Id)
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .Select(factura => new PresupuestoDto
                {
                    Id = factura.Id,
                    ClienteId = factura.ClienteId,
                    TrabajoId = factura.TrabajoId,
                    NumeroDocumento = factura.NumeroFactura,
                    FechaEmision = factura.FechaEmision,
                    FechaVencimiento = factura.FechaVencimiento,
                    Subtotal = factura.Subtotal,
                    ImporteImpuestos = factura.ImporteImpuestos,
                    Total = factura.Total,
                    EstadoPago = factura.EstadoPago,
                    MetodoPago = factura.MetodoPago,
                    NotasLegales = factura.NotasLegales,
                    ClienteNombre = factura.ClienteNombre,
                    ClienteNifCif = factura.ClienteNifCif,
                    ClienteDireccion = factura.ClienteDireccion,
                    ClienteCodigoPostal = factura.ClienteCodigoPostal,
                    ClienteLocalidad = factura.ClienteLocalidad,
                    ClienteProvincia = factura.ClienteProvincia
                })
                .ToListAsync();

            PagedResponse<PresupuestoDto> respuesta = new PagedResponse<PresupuestoDto>
            {
                Data = data,
                PageNumber = paginacion.PageNumber,
                PageSize = paginacion.PageSize,
                TotalCount = totalCount
            };

            return respuesta;
        }

        public async Task<PresupuestoDto?> ObtenerDetallePorIdAsync(int presupuestoId)
        {
            Factura? factura = await _context.Facturas
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == presupuestoId && item.TipoDocumento == TipoDocumentoComercial.PRESUPUESTO);

            if (factura == null)
            {
                return null;
            }

            List<LineaPresupuestoDto> lineas = await _context.LineasFactura
                .AsNoTracking()
                .Where(linea => linea.FacturaId == presupuestoId)
                .OrderBy(linea => linea.Id)
                .Select(linea => new LineaPresupuestoDto
                {
                    Id = linea.Id,
                    ServicioId = linea.ServicioId,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = linea.PrecioUnitario,
                    DescuentoPorcentaje = linea.DescuentoPorcentaje,
                    ImpuestoPorcentaje = linea.ImpuestoPorcentaje,
                    SubtotalLinea = linea.SubtotalLinea,
                    TotalLinea = linea.TotalLinea
                })
                .ToListAsync();

            PresupuestoDto dto = new PresupuestoDto
            {
                Id = factura.Id,
                ClienteId = factura.ClienteId,
                TrabajoId = factura.TrabajoId,
                NumeroDocumento = factura.NumeroFactura,
                FechaEmision = factura.FechaEmision,
                FechaVencimiento = factura.FechaVencimiento,
                Subtotal = factura.Subtotal,
                ImporteImpuestos = factura.ImporteImpuestos,
                Total = factura.Total,
                EstadoPago = factura.EstadoPago,
                MetodoPago = factura.MetodoPago,
                NotasLegales = factura.NotasLegales,
                ClienteNombre = factura.ClienteNombre,
                ClienteNifCif = factura.ClienteNifCif,
                ClienteDireccion = factura.ClienteDireccion,
                ClienteCodigoPostal = factura.ClienteCodigoPostal,
                ClienteLocalidad = factura.ClienteLocalidad,
                ClienteProvincia = factura.ClienteProvincia,
                Lineas = lineas
            };

            return dto;
        }

        public async Task<Factura?> ObtenerEntidadPorIdAsync(int presupuestoId)
        {
            Factura? factura = await _context.Facturas.FindAsync(presupuestoId);
            return factura;
        }

        public async Task AddAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva>? desglosesIva = null)
        {
            await _context.Facturas.AddAsync(factura);
            await _context.SaveChangesAsync();

            foreach (LineaFactura linea in lineas)
            {
                linea.FacturaId = factura.Id;
            }

            await _context.LineasFactura.AddRangeAsync(lineas);

            if (desglosesIva?.Count > 0)
            {
                foreach (DesgloseIva desglose in desglosesIva)
                {
                    desglose.FacturaId = factura.Id;
                }
                await _context.DesglosesIva.AddRangeAsync(desglosesIva);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerarNumeroDocumentoAsync(int tallerId, TipoDocumentoComercial tipoDocumento)
        {
            SqlParameter parametroTallerId = new SqlParameter("@TallerId", tallerId);
            SqlParameter parametroTipoDocumento = new SqlParameter("@TipoDocumento", tipoDocumento.ToString());
            SqlParameter parametroNumero = new SqlParameter("@NumeroGenerado", System.Data.SqlDbType.NVarChar, 100)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_SiguienteNumeroDocumento] @TallerId, @TipoDocumento, @NumeroGenerado OUTPUT",
                parametroTallerId,
                parametroTipoDocumento,
                parametroNumero);

            string numero = parametroNumero.Value?.ToString() ?? string.Empty;
            return numero;
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            bool pertenece = await _context.Facturas
                .AsNoTracking()
                .AnyAsync(factura => factura.Id == id && factura.TallerId == tallerId && factura.TipoDocumento == TipoDocumentoComercial.PRESUPUESTO);

            return pertenece;
        }
    }
}
