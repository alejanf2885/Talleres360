using Talleres360.Dtos;
using Talleres360.Dtos.Inventario;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;

namespace Talleres360.Interfaces.Inventario
{
    public interface IProductoRepository : ITallerRecursoRepository
    {
        Task<PagedResponse<ProductoDto>> ObtenerProductosPagedAsync(int tallerId, PaginationParams paginacion, string? buscar = null, int? categoriaId = null);
        Task<ProductoDto?> ObtenerDetallePorIdAsync(int productoId);
        Task<Producto?> ObtenerEntidadPorIdAsync(int productoId);
        Task<bool> ExisteNombreAsync(int tallerId, string nombre, int? productoExcluirId = null);
        Task<bool> ExisteReferenciaAsync(int tallerId, string referencia, int? productoExcluirId = null);
        Task AddAsync(Producto producto);
        Task UpdateAsync(Producto producto);
    }
}
