using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Facturacion;
using Talleres360.Dtos.Responses;
using Talleres360.Interfaces.Facturacion;

namespace Talleres360.Repositories.Facturas
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly ApplicationDbContext _context;

        public FacturaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerarNumeroFacturaAsync(int tallerId)
        {
            SqlParameter pTallerId = new SqlParameter("@p_TallerId", tallerId);
            SqlParameter pTipo     = new SqlParameter("@p_Tipo", "FACTURA");
            SqlParameter pNumero   = new SqlParameter("@p_Numero", SqlDbType.NVarChar, 100)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_SiguienteNumeroDocumento] @TallerId = @p_TallerId, @TipoDocumento = @p_Tipo, @NumeroGenerado = @p_Numero OUTPUT",
                pTallerId, pTipo, pNumero);

            return pNumero.Value?.ToString() ?? string.Empty;
        }

        public async Task<List<DetalleTrabajo>> ObtenerDetallesParaFacturarAsync(int trabajoId)
        {
            return await _context.DetallesTrabajo
                .Where(d => d.TrabajoId == trabajoId && !d.Eliminado)
                .ToListAsync();
        }

        public async Task GuardarSnapshotAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            await _context.Facturas.AddAsync(factura);
            await _context.SaveChangesAsync();

            lineas.ForEach(l => l.FacturaId = factura.Id);
            desgloses.ForEach(d => d.FacturaId = factura.Id);

            await _context.LineasFactura.AddRangeAsync(lineas);
            await _context.DesglosesIva.AddRangeAsync(desgloses);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
        }

        public async Task ActualizarUrlPdfAsync(int facturaId, string urlPdf)
        {
            Factura? factura = await _context.Facturas.FindAsync(facturaId);
            if (factura == null) return;
            factura.UrlPdf = urlPdf;
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResponse<FacturaDto>> ObtenerTodosPorTallerAsync(int tallerId, PaginationParams pagination)
        {
            IQueryable<Factura> query = _context.Facturas
                .Where(f => f.TallerId == tallerId)
                .OrderByDescending(f => f.FechaEmision);

            int total = await query.CountAsync();

            List<FacturaDto> items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .AsNoTracking()
                .Select(f => new FacturaDto
                {
                    Id = f.Id, TallerId = f.TallerId, TrabajoId = f.TrabajoId, ClienteId = f.ClienteId,
                    NumeroFactura = f.NumeroFactura, TipoDocumento = f.TipoDocumento,
                    FechaEmision = f.FechaEmision, FechaVencimiento = f.FechaVencimiento,
                    Subtotal = f.Subtotal, ImporteImpuestos = f.ImporteImpuestos, Total = f.Total,
                    EstadoPago = f.EstadoPago, MetodoPago = f.MetodoPago, UrlPdf = f.UrlPdf,
                    ClienteNombre = f.ClienteNombre, ClienteNifCif = f.ClienteNifCif,
                    ClienteEmail = f.ClienteEmail, ClienteTelefono = f.ClienteTelefono,
                    TallerNombre = f.TallerNombre, TallerCif = f.TallerCif
                })
                .ToListAsync();

            return new PagedResponse<FacturaDto>
            {
                Data = items, PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize, TotalCount = total
            };
        }

        public async Task<FacturaDto?> ObtenerPorIdAsync(int facturaId, int tallerId)
        {
            Factura? f = await _context.Facturas
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == facturaId && f.TallerId == tallerId);

            if (f == null) return null;
            return await MapToDtoConLineasAsync(f);
        }

        public async Task<FacturaDto?> ObtenerPorTrabajoAsync(int trabajoId, int tallerId)
        {
            Factura? f = await _context.Facturas
                .AsNoTracking()
                .Where(f => f.TrabajoId == trabajoId && f.TallerId == tallerId)
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            if (f == null) return null;
            return await MapToDtoConLineasAsync(f);
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId) =>
            await _context.Facturas.AnyAsync(f => f.Id == id && f.TallerId == tallerId);

        private async Task<FacturaDto> MapToDtoConLineasAsync(Factura f)
        {
            List<LineaFacturaDto> lineas = await _context.LineasFactura
                .Where(l => l.FacturaId == f.Id)
                .AsNoTracking()
                .Select(l => new LineaFacturaDto
                {
                    Id = l.Id, ServicioId = l.ServicioId, Concepto = l.Concepto,
                    Cantidad = l.Cantidad, PrecioUnitario = l.PrecioUnitario,
                    DescuentoPorcentaje = l.DescuentoPorcentaje, ImpuestoPorcentaje = l.ImpuestoPorcentaje,
                    SubtotalLinea = l.SubtotalLinea, TotalLinea = l.TotalLinea
                })
                .ToListAsync();

            return new FacturaDto
            {
                Id = f.Id, TallerId = f.TallerId, TrabajoId = f.TrabajoId, ClienteId = f.ClienteId,
                NumeroFactura = f.NumeroFactura, TipoDocumento = f.TipoDocumento,
                FechaEmision = f.FechaEmision, FechaVencimiento = f.FechaVencimiento,
                Subtotal = f.Subtotal, ImporteImpuestos = f.ImporteImpuestos, Total = f.Total,
                EstadoPago = f.EstadoPago, MetodoPago = f.MetodoPago, UrlPdf = f.UrlPdf,
                ClienteNombre = f.ClienteNombre, ClienteNifCif = f.ClienteNifCif,
                ClienteEmail = f.ClienteEmail, ClienteTelefono = f.ClienteTelefono,
                TallerNombre = f.TallerNombre, TallerCif = f.TallerCif,
                Lineas = lineas
            };
        }
    }
}
