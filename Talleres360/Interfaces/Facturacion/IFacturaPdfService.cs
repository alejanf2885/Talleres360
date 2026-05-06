using Talleres360.Models.Facturacion;

namespace Talleres360.Interfaces.Facturacion
{
    public interface IFacturaPdfService
    {
        Task<string> GenerarYSubirAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses, string? logoBase64 = null);
    }
}
