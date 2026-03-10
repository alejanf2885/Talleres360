using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Models;

namespace Talleres360.Interfaces.Clientes
{
    public interface ICustomerService
    {
        Task<IEnumerable<Cliente>> ObtenerTodosAsync(int tallerId, string? buscar = null);
        Task<PagedResponse<Cliente>> ObtenerTodosPagedAsync(int tallerId, PaginationParams pagination, string? buscar = null);
        Task<Cliente?> ObtenerPorIdAsync(int tallerId, int clienteId);
        Task<ClienteStatsResponse> ObtenerEstadisticasAsync(int tallerId);

        Task<(bool Success, string Message, Cliente? Cliente)> CrearClienteAsync(int tallerId, CrearClienteRequest request);
        Task<(bool Success, string Message, Cliente? Cliente)> ActualizarClienteAsync(int tallerId, int clienteId, ActualizarClienteRequest request);
        Task<(bool Success, string Message)> EliminarClienteAsync(int tallerId, int clienteId);
    }

}

