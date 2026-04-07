using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Inventario;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Inventario;
using Talleres360.Interfaces.Seguridad;

namespace Talleres360.Controllers
{
    [Route("api/v1/inventario/productos")]
    [ApiController]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly IUserContextService _userContextService;

        public ProductosController(
            IProductoService productoService,
            IUserContextService userContextService)
        {
            _productoService = productoService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paginacion,
            [FromQuery] string? buscar = null,
            [FromQuery] int? categoriaId = null)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            PagedResponse<ProductoDto> productos = await _productoService.ObtenerProductosAsync(tallerId.Value, paginacion, buscar, categoriaId);

            return Ok(ApiResponse<PagedResponse<ProductoDto>>.Ok(productos, "Productos recuperados correctamente."));
        }

        [TallerAuthorize<IProductoRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<ProductoDto> resultado = await _productoService.ObtenerProductoPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.INV_PRODUCTO_NO_ENCONTRADO.ToString(),
                    mensaje: resultado.Message ?? "Producto no encontrado."));
            }

            return Ok(ApiResponse<ProductoDto>.Ok(resultado.Data!, "Producto recuperado correctamente."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearProductoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<ProductoDto> resultado = await _productoService.CrearProductoAsync(tallerId.Value, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al crear producto."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<ProductoDto>.Ok(resultado.Data, "¡Producto creado con éxito!"));
        }

        [TallerAuthorize<IProductoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarProductoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<ProductoDto> resultado = await _productoService.ActualizarProductoAsync(tallerId.Value, id, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar producto."));
            }

            return Ok(ApiResponse<ProductoDto>.Ok(resultado.Data!, "Producto actualizado correctamente."));
        }

        [TallerAuthorize<IProductoRepository>]
        [RequiereSuscripcionActiva]
        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _productoService.EliminarProductoAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar producto."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Producto archivado correctamente."));
        }
    }
}
