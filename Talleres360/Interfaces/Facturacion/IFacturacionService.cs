using Talleres360.Dtos;
using Talleres360.Dtos.Facturacion;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;

namespace Talleres360.Interfaces.Facturacion
{
    public interface IFacturacionService
    {
        Task<ServiceResult<TrabajoDto>> FacturarTrabajoAsync(int tallerId, int trabajoId);
        Task<ServiceResult<PagedResponse<FacturaDto>>> ObtenerTodosAsync(int tallerId, PaginationParams pagination);
        Task<ServiceResult<FacturaDto>> ObtenerPorIdAsync(int tallerId, int facturaId);
        Task<ServiceResult<FacturaDto>> ObtenerPorTrabajoAsync(int tallerId, int trabajoId);
        Task<ServiceResult<FacturaDto>> RegenerarPdfAsync(int tallerId, int facturaId);
    }
}
