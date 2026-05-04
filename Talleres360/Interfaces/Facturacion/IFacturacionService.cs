using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;

namespace Talleres360.Interfaces.Facturacion
{
    public interface IFacturacionService
    {
        Task<ServiceResult<TrabajoDto>> FacturarTrabajoAsync(int tallerId, int trabajoId);
    }
}
