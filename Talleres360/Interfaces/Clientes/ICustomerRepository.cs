using Talleres360.Models;

namespace Talleres360.Interfaces.Clientes
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Cliente>> GetAllByTallerIdAsync(int tallerId, string? buscar = null);
        Task<Cliente?> GetByIdAsync(int id);
        Task<int> CountByTallerIdAsync(int tallerId);
        Task AddAsync(Cliente cliente);
    }
}
