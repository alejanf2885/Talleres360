using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Talleres360.Data;
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
            await _context.Facturas.AddAsync(factura);
            await _context.SaveChangesAsync();

            lineas.ForEach(l => l.FacturaId = factura.Id);
            desgloses.ForEach(d => d.FacturaId = factura.Id);

            await _context.LineasFactura.AddRangeAsync(lineas);
            await _context.DesglosesIva.AddRangeAsync(desgloses);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarUrlPdfAsync(int facturaId, string urlPdf)
        {
            Factura? factura = await _context.Facturas.FindAsync(facturaId);
            if (factura == null) return;
            factura.UrlPdf = urlPdf;
            await _context.SaveChangesAsync();
        }
    }
}
