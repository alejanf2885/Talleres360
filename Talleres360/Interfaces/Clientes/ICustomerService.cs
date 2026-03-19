using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Dtos.Responses; // <-- Asegúrate de tener tu ServiceResult aquí
using Talleres360.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Talleres360.Interfaces.Clientes
{
    public interface ICustomerService
    {
        Task<IEnumerable<Cliente>> ObtenerTodosAsync(int tallerId, string? buscar = null);
        Task<PagedResponse<Cliente>> ObtenerTodosPagedAsync(int tallerId, PaginationParams pagination, string? buscar = null);
        Task<Cliente?> ObtenerPorIdAsync(int tallerId, int clienteId);
        Task<ClienteStatsResponse> ObtenerEstadisticasAsync(int tallerId);

        Task<ServiceResult<Cliente>> CrearClienteAsync(int tallerId, CrearClienteRequest request);
        Task<ServiceResult<Cliente>> ActualizarClienteAsync(int tallerId, int clienteId, ActualizarClienteRequest request);
        Task<ServiceResult<bool>> EliminarClienteAsync(int tallerId, int clienteId);
    }
}