using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Interfaces.Inventario
{
    public interface ICategoriaProductoRepository : ITallerRecursoRepository
    {
        Task<List<CategoriaProducto>> ObtenerCategoriasAsync(int tallerId);
        Task<CategoriaProducto?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteNombreAsync(int tallerId, string nombre, int? categoriaExcluirId = null);
        Task AddAsync(CategoriaProducto categoria);
        Task UpdateAsync(CategoriaProducto categoria);
    }
}
