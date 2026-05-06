using Talleres360.Dtos;
using Talleres360.Dtos.Facturacion;
using Talleres360.Dtos.Responses;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Interfaces.Facturacion
{
    public interface IFacturaRepository : ITallerRecursoRepository
    {
        Task<string> GenerarNumeroFacturaAsync(int tallerId);
        Task<List<DetalleTrabajo>> ObtenerDetallesParaFacturarAsync(int trabajoId);
        Task GuardarSnapshotAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses);
        Task ActualizarUrlPdfAsync(int facturaId, string urlPdf);
        Task<PagedResponse<FacturaDto>> ObtenerTodosPorTallerAsync(int tallerId, PaginationParams pagination);
        Task<FacturaDto?> ObtenerPorIdAsync(int facturaId, int tallerId);
        Task<FacturaDto?> ObtenerPorTrabajoAsync(int trabajoId, int tallerId);
        Task<bool> PerteneceATallerAsync(int id, int tallerId);
        Task<(Factura? Factura, List<LineaFactura> Lineas, List<DesgloseIva> Desgloses)> ObtenerEntidadParaPdfAsync(int facturaId, int tallerId);
    }
}
