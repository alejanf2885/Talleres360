using Talleres360.Dtos;
using Talleres360.Dtos.Inventario;
using Talleres360.Dtos.Responses;

namespace Talleres360.Interfaces.Inventario
{
    public interface IProductoService
    {
        Task<PagedResponse<ProductoDto>> ObtenerProductosAsync(int tallerId, PaginationParams paginacion, string? buscar = null, int? categoriaId = null);
        Task<ServiceResult<ProductoDto>> ObtenerProductoPorIdAsync(int tallerId, int productoId);
        Task<ServiceResult<ProductoDto>> CrearProductoAsync(int tallerId, CrearProductoRequest request);
        Task<ServiceResult<ProductoDto>> ActualizarProductoAsync(int tallerId, int productoId, ActualizarProductoRequest request);
        Task<ServiceResult<bool>> EliminarProductoAsync(int tallerId, int productoId);
    }
}
