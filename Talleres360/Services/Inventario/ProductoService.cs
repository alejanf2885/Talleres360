using Talleres360.Dtos;
using Talleres360.Dtos.Inventario;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Inventario;

namespace Talleres360.Services.Inventario
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly ICategoriaProductoRepository _categoriaRepository;

        public ProductoService(
            IProductoRepository productoRepository,
            ICategoriaProductoRepository categoriaRepository)
        {
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<PagedResponse<ProductoDto>> ObtenerProductosAsync(int tallerId, PaginationParams paginacion, string? buscar = null, int? categoriaId = null)
        {
            PagedResponse<ProductoDto> productos = await _productoRepository.ObtenerProductosPagedAsync(tallerId, paginacion, buscar, categoriaId);
            return productos;
        }

        public async Task<ServiceResult<ProductoDto>> ObtenerProductoPorIdAsync(int tallerId, int productoId)
        {
            ProductoDto? producto = await _productoRepository.ObtenerDetallePorIdAsync(productoId);
            if (producto == null)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_PRODUCTO_NO_ENCONTRADO.ToString(),
                    "Producto no encontrado.");
            }

            Producto? entidad = await _productoRepository.ObtenerEntidadPorIdAsync(productoId);
            if (entidad == null || entidad.TallerId != tallerId || entidad.Eliminado)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_PRODUCTO_NO_ENCONTRADO.ToString(),
                    "Producto no encontrado.");
            }

            return ServiceResult<ProductoDto>.Ok(producto);
        }

        public async Task<ServiceResult<ProductoDto>> CrearProductoAsync(int tallerId, CrearProductoRequest request)
        {
            CategoriaProducto? categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId);
            if (categoria == null || categoria.TallerId != tallerId || categoria.Eliminado)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_CATEGORIA_NO_ENCONTRADA.ToString(),
                    "La categoría indicada no existe en el taller.");
            }

            if (request.PrecioVenta < request.PrecioCompra)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_PRECIO_INVALIDO.ToString(),
                    "El precio de venta no puede ser menor que el precio de compra.");
            }

            string nombreNormalizado = request.Nombre.Trim().ToUpper();
            bool existeNombre = await _productoRepository.ExisteNombreAsync(tallerId, nombreNormalizado);
            if (existeNombre)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_PRODUCTO_NOMBRE_DUPLICADO.ToString(),
                    "Ya existe un producto con ese nombre en el taller.");
            }

            string? referenciaNormalizada = string.IsNullOrWhiteSpace(request.Referencia)
                ? null
                : request.Referencia.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(referenciaNormalizada))
            {
                bool existeReferencia = await _productoRepository.ExisteReferenciaAsync(tallerId, referenciaNormalizada);
                if (existeReferencia)
                {
                    return ServiceResult<ProductoDto>.Fail(
                        ErrorCode.INV_PRODUCTO_REFERENCIA_DUPLICADA.ToString(),
                        "Ya existe un producto con esa referencia en el taller.");
                }
            }

            Producto producto = new Producto
            {
                TallerId = tallerId,
                CategoriaId = request.CategoriaId,
                Referencia = referenciaNormalizada,
                Nombre = nombreNormalizado,
                PrecioCompra = request.PrecioCompra,
                PrecioVenta = request.PrecioVenta,
                StockActual = request.StockActual,
                ControlarStock = request.ControlarStock,
                Eliminado = false
            };

            await _productoRepository.AddAsync(producto);

            ProductoDto? detalle = await _productoRepository.ObtenerDetallePorIdAsync(producto.Id);
            if (detalle == null)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Producto creado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<ProductoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<ProductoDto>> ActualizarProductoAsync(int tallerId, int productoId, ActualizarProductoRequest request)
        {
            Producto? producto = await _productoRepository.ObtenerEntidadPorIdAsync(productoId);
            if (producto == null || producto.TallerId != tallerId || producto.Eliminado)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_PRODUCTO_NO_ENCONTRADO.ToString(),
                    "Producto no encontrado.");
            }

            CategoriaProducto? categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId);
            if (categoria == null || categoria.TallerId != tallerId || categoria.Eliminado)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_CATEGORIA_NO_ENCONTRADA.ToString(),
                    "La categoría indicada no existe en el taller.");
            }

            if (request.PrecioVenta < request.PrecioCompra)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_PRECIO_INVALIDO.ToString(),
                    "El precio de venta no puede ser menor que el precio de compra.");
            }

            string nombreNormalizado = request.Nombre.Trim().ToUpper();
            bool existeNombre = await _productoRepository.ExisteNombreAsync(tallerId, nombreNormalizado, productoId);
            if (existeNombre)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.INV_PRODUCTO_NOMBRE_DUPLICADO.ToString(),
                    "Ya existe otro producto con ese nombre en el taller.");
            }

            string? referenciaNormalizada = string.IsNullOrWhiteSpace(request.Referencia)
                ? null
                : request.Referencia.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(referenciaNormalizada))
            {
                bool existeReferencia = await _productoRepository.ExisteReferenciaAsync(tallerId, referenciaNormalizada, productoId);
                if (existeReferencia)
                {
                    return ServiceResult<ProductoDto>.Fail(
                        ErrorCode.INV_PRODUCTO_REFERENCIA_DUPLICADA.ToString(),
                        "Ya existe otro producto con esa referencia en el taller.");
                }
            }

            producto.CategoriaId = request.CategoriaId;
            producto.Referencia = referenciaNormalizada;
            producto.Nombre = nombreNormalizado;
            producto.PrecioCompra = request.PrecioCompra;
            producto.PrecioVenta = request.PrecioVenta;
            producto.StockActual = request.StockActual;
            producto.ControlarStock = request.ControlarStock;

            await _productoRepository.UpdateAsync(producto);

            ProductoDto? detalle = await _productoRepository.ObtenerDetallePorIdAsync(productoId);
            if (detalle == null)
            {
                return ServiceResult<ProductoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Producto actualizado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<ProductoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<bool>> EliminarProductoAsync(int tallerId, int productoId)
        {
            Producto? producto = await _productoRepository.ObtenerEntidadPorIdAsync(productoId);
            if (producto == null || producto.TallerId != tallerId || producto.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.INV_PRODUCTO_NO_ENCONTRADO.ToString(),
                    "Producto no encontrado.");
            }

            producto.Eliminado = true;
            await _productoRepository.UpdateAsync(producto);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
