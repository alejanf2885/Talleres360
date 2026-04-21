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
            SqlParameter parametroTallerId = new SqlParameter("@TallerId", tallerId);
            SqlParameter parametroTipo = new SqlParameter("@TipoDocumento", "FACTURA");
            SqlParameter parametroNumero = new SqlParameter("@NumeroGenerado", SqlDbType.NVarChar, 100)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_SiguienteNumeroDocumento] @TallerId, @TipoDocumento, @NumeroGenerado OUTPUT",
                parametroTallerId, parametroTipo, parametroNumero);

            return parametroNumero.Value?.ToString() ?? string.Empty;
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
    }
}
