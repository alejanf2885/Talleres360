using Talleres360.Dtos.Inventario;
using Talleres360.Dtos.Responses;

namespace Talleres360.Interfaces.Inventario
{
    public interface ICategoriaProductoService
    {
        Task<ServiceResult<List<CategoriaProductoDto>>> ObtenerCategoriasAsync(int tallerId);
        Task<ServiceResult<CategoriaProductoDto>> ObtenerPorIdAsync(int tallerId, int categoriaId);
        Task<ServiceResult<CategoriaProductoDto>> CrearCategoriaAsync(int tallerId, CrearCategoriaProductoRequest request);
        Task<ServiceResult<CategoriaProductoDto>> ActualizarCategoriaAsync(int tallerId, int categoriaId, ActualizarCategoriaProductoRequest request);
        Task<ServiceResult<bool>> EliminarCategoriaAsync(int tallerId, int categoriaId);
    }
}
