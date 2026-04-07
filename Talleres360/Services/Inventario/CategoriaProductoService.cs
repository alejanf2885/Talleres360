using Talleres360.Dtos.Inventario;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Inventario;
using Talleres360.Models;

namespace Talleres360.Services.Inventario
{
    public class CategoriaProductoService : ICategoriaProductoService
    {
        private readonly ICategoriaProductoRepository _categoriaRepository;
        private readonly IProductoRepository _productoRepository;

        public CategoriaProductoService(
            ICategoriaProductoRepository categoriaRepository,
            IProductoRepository productoRepository)
        {
            _categoriaRepository = categoriaRepository;
            _productoRepository = productoRepository;
        }

        public async Task<ServiceResult<List<CategoriaProductoDto>>> ObtenerCategoriasAsync(int tallerId)
        {
            if (tallerId <= 0)
            {
                return ServiceResult<List<CategoriaProductoDto>>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller debe ser mayor a 0.");
            }

            List<CategoriaProducto> categorias = await _categoriaRepository.ObtenerCategoriasAsync(tallerId);

            List<CategoriaProductoDto> data = categorias
                .Select(categoria => new CategoriaProductoDto
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre
                })
                .ToList();

            return ServiceResult<List<CategoriaProductoDto>>.Ok(data);
        }

        public async Task<ServiceResult<CategoriaProductoDto>> ObtenerPorIdAsync(int tallerId, int categoriaId)
        {
            if (tallerId <= 0 || categoriaId <= 0)
            {
                return ServiceResult<CategoriaProductoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "Los identificadores enviados no son válidos.");
            }

            CategoriaProducto? categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId);
            if (categoria == null || categoria.TallerId != tallerId)
            {
                return ServiceResult<CategoriaProductoDto>.Fail(
                    ErrorCode.INV_CATEGORIA_NO_ENCONTRADA.ToString(),
                    "Categoría no encontrada.");
            }

            CategoriaProductoDto data = new CategoriaProductoDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre
            };

            return ServiceResult<CategoriaProductoDto>.Ok(data);
        }

        public async Task<ServiceResult<CategoriaProductoDto>> CrearCategoriaAsync(int tallerId, CrearCategoriaProductoRequest request)
        {
            if (tallerId <= 0)
            {
                return ServiceResult<CategoriaProductoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller debe ser mayor a 0.");
            }

            string nombreNormalizado = request.Nombre.Trim().ToUpper();

            bool existeNombre = await _categoriaRepository.ExisteNombreAsync(tallerId, nombreNormalizado);
            if (existeNombre)
            {
                return ServiceResult<CategoriaProductoDto>.Fail(
                    ErrorCode.INV_CATEGORIA_NOMBRE_DUPLICADO.ToString(),
                    "Ya existe una categoría con ese nombre.");
            }

            CategoriaProducto categoria = new CategoriaProducto
            {
                TallerId = tallerId,
                Nombre = nombreNormalizado,
                Eliminado = false
            };

            await _categoriaRepository.AddAsync(categoria);

            CategoriaProductoDto data = new CategoriaProductoDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre
            };

            return ServiceResult<CategoriaProductoDto>.Ok(data);
        }

        public async Task<ServiceResult<CategoriaProductoDto>> ActualizarCategoriaAsync(int tallerId, int categoriaId, ActualizarCategoriaProductoRequest request)
        {
            CategoriaProducto? categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId);
            if (categoria == null || categoria.TallerId != tallerId)
            {
                return ServiceResult<CategoriaProductoDto>.Fail(
                    ErrorCode.INV_CATEGORIA_NO_ENCONTRADA.ToString(),
                    "Categoría no encontrada.");
            }

            string nombreNormalizado = request.Nombre.Trim().ToUpper();

            bool existeNombre = await _categoriaRepository.ExisteNombreAsync(tallerId, nombreNormalizado, categoriaId);
            if (existeNombre)
            {
                return ServiceResult<CategoriaProductoDto>.Fail(
                    ErrorCode.INV_CATEGORIA_NOMBRE_DUPLICADO.ToString(),
                    "Ya existe otra categoría con ese nombre.");
            }

            categoria.Nombre = nombreNormalizado;
            await _categoriaRepository.UpdateAsync(categoria);

            CategoriaProductoDto data = new CategoriaProductoDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre
            };

            return ServiceResult<CategoriaProductoDto>.Ok(data);
        }

        public async Task<ServiceResult<bool>> EliminarCategoriaAsync(int tallerId, int categoriaId)
        {
            CategoriaProducto? categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId);
            if (categoria == null || categoria.TallerId != tallerId)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.INV_CATEGORIA_NO_ENCONTRADA.ToString(),
                    "Categoría no encontrada.");
            }

            PagedResponse<ProductoDto> productos = await _productoRepository.ObtenerProductosPagedAsync(
                tallerId,
                new PaginationParams { PageNumber = 1, PageSize = 5 },
                null,
                categoriaId);

            if (productos.TotalCount > 0)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "No se puede eliminar la categoría porque tiene productos asociados.");
            }

            categoria.Eliminado = true;
            await _categoriaRepository.UpdateAsync(categoria);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
