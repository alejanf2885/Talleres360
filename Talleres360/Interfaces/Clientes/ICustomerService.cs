using Talleres360.Dtos.Clientes;
using Talleres360.Models;

namespace Talleres360.Interfaces.Clientes
{
    public interface ICustomerService
    {
        Task<(bool Success, string Message, Cliente? Cliente)> CrearClienteAsync(int tallerId, CrearClienteRequest request);
        Task<IEnumerable<Cliente>> ObtenerTodosAsync(int tallerId, string? buscar = null);
        Task<Cliente?> ObtenerPorIdAsync(int tallerId, int clienteId);
    }
}

