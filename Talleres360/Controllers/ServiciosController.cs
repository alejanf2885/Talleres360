using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Servicios;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Servicios;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiciosController : ControllerBase
    {
        private readonly IServicioService _servicioService;
        private readonly IUserContextService _userContextService;

        public ServiciosController(IServicioService servicioService, IUserContextService userContextService)
        {
            _servicioService = servicioService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paginacion,
            [FromQuery] string? buscar = null,
            [FromQuery] bool? activo = null)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            PagedResponse<ServicioDto> servicios = await _servicioService.ObtenerTodosAsync(tallerId.Value, paginacion, buscar, activo);
            return Ok(ApiResponse<PagedResponse<ServicioDto>>.Ok(servicios, "Listado de servicios recuperado correctamente."));
        }

        [TallerAuthorize<IServicioRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<ServicioDto> resultado = await _servicioService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Servicio no encontrado."));
            }

            return Ok(ApiResponse<ServicioDto>.Ok(resultado.Data!, "Servicio recuperado correctamente."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearServicioRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<ServicioDto> resultado = await _servicioService.CrearAsync(tallerId.Value, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo crear el servicio."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<ServicioDto>.Ok(resultado.Data, "Servicio creado correctamente."));
        }

        [TallerAuthorize<IServicioRepository>]
        [RequiereSuscripcionActiva]
        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarServicioRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<ServicioDto> resultado = await _servicioService.ActualizarAsync(tallerId.Value, id, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo actualizar el servicio."));
            }

            return Ok(ApiResponse<ServicioDto>.Ok(resultado.Data!, "Servicio actualizado correctamente."));
        }

        [TallerAuthorize<IServicioRepository>]
        [RequiereSuscripcionActiva]
        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _servicioService.EliminarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo eliminar el servicio."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Servicio eliminado correctamente."));
        }
    }
}
