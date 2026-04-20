using Talleres360.Dtos;
using Talleres360.Dtos.Presupuestos;
using Talleres360.Enums;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.Interfaces.Presupuestos
{
    public interface IPresupuestoRepository : ITallerRecursoRepository
    {
        Task<PagedResponse<PresupuestoDto>> ObtenerTodosPagedAsync(int tallerId, PaginationParams paginacion);
        Task<PresupuestoDto?> ObtenerDetallePorIdAsync(int presupuestoId);
        Task<Factura?> ObtenerEntidadPorIdAsync(int presupuestoId);
        Task AddAsync(Factura factura, List<LineaFactura> lineas, List<DesgloseIva>? desglosesIva = null);
        Task<string> GenerarNumeroDocumentoAsync(int tallerId, TipoDocumentoComercial tipoDocumento);
    }
}
