using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Dtos.Responses;
using Talleres360.Models;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IVehiculoService
    {
        Task<ServiceResult<VehiculoDetalle>> RegistrarVehiculoAsync(int tallerId, Vehiculo vehiculo);
        Task<ServiceResult<VehiculoDetalle>> ActualizarVehiculoAsync(int tallerId, int id, Vehiculo vehiculo);
        Task<ServiceResult<VehiculoDetalle>> GetDetalleByIdAsync(int tallerId, int id);
        Task<ServiceResult<VehiculoDetalle>> GetDetalleByMatriculaAsync(int tallerId, string matricula);

        Task<PagedResponse<VehiculoDetalle>> GetAllDetalleByTallerPagedAsync(
            int tallerId,
            int pageNumber,
            int pageSize,
            VehiculoFiltroDto? filtro = null);
    }
}