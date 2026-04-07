using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos.Inventario;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Inventario;
using Talleres360.Interfaces.Seguridad;

namespace Talleres360.Controllers
{
    [Route("api/v1/inventario/categorias")]
    [ApiController]
    [Authorize]
    public class CategoriasProductoController : ControllerBase
    {
        private readonly ICategoriaProductoService _categoriaService;
        private readonly IUserContextService _userContextService;

        public CategoriasProductoController(
            ICategoriaProductoService categoriaService,
            IUserContextService userContextService)
        {
            _categoriaService = categoriaService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<List<CategoriaProductoDto>> resultado = await _categoriaService.ObtenerCategoriasAsync(tallerId.Value);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al obtener categorías."));
            }

            List<CategoriaProductoDto> categorias = resultado.Data ?? new List<CategoriaProductoDto>();

            return Ok(ApiResponse<List<CategoriaProductoDto>>.Ok(categorias, "Categorías recuperadas correctamente."));
        }

        [TallerAuthorize<ICategoriaProductoRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<CategoriaProductoDto> resultado = await _categoriaService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.INV_CATEGORIA_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Categoría no encontrada."));
            }

            return Ok(ApiResponse<CategoriaProductoDto>.Ok(resultado.Data!, "Categoría recuperada correctamente."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearCategoriaProductoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<CategoriaProductoDto> resultado = await _categoriaService.CrearCategoriaAsync(tallerId.Value, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al crear categoría."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<CategoriaProductoDto>.Ok(resultado.Data, "¡Categoría creada con éxito!"));
        }

        [TallerAuthorize<ICategoriaProductoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarCategoriaProductoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<CategoriaProductoDto> resultado = await _categoriaService.ActualizarCategoriaAsync(tallerId.Value, id, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar categoría."));
            }

            return Ok(ApiResponse<CategoriaProductoDto>.Ok(resultado.Data!, "Categoría actualizada correctamente."));
        }

        [TallerAuthorize<ICategoriaProductoRepository>]
        [RequiereSuscripcionActiva]
        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _categoriaService.EliminarCategoriaAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar categoría."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Categoría eliminada correctamente."));
        }
    }
}
