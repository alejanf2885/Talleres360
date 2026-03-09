using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Interfaces.Clientes;
using Talleres360.Models;
using System.Linq;

namespace Talleres360.Repositories.Clientes
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cliente>> GetAllByTallerIdAsync(int tallerId, string? buscar = null)
        {
            var query = _context.Clientes
               .Where(c => c.TallerId == tallerId && !c.Eliminado);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string criterio = buscar.Trim().ToLower();
                query = query.Where(c =>
                    c.Nombre.Contains(criterio) ||
                    (c.Apellidos != null && c.Apellidos.Contains(criterio)) ||
                    c.Telefono.Contains(criterio) ||
                    (c.Email != null && c.Email.Contains(criterio))
                );
            }

            List<Cliente> resultado = await query
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();
            return resultado;
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            Cliente? cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && !c.Eliminado);

            return cliente;
        }

        public async Task<int> CountByTallerIdAsync(int tallerId)
        {
            int total = await _context.Clientes
                .CountAsync(c => c.TallerId == tallerId && !c.Eliminado);

            return total;
        }

        public async Task AddAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Cliente? cliente = await GetByIdAsync(id);
            if (cliente != null)
            {
                cliente.Eliminado = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}