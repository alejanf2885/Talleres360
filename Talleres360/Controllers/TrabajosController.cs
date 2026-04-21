using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TrabajosController : ControllerBase
    {
        private readonly ITrabajoService _trabajoService;
        private readonly IUserContextService _userContextService;

        public TrabajosController(ITrabajoService trabajoService, IUserContextService userContextService)
        {
            _trabajoService = trabajoService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paginacion,
            [FromQuery] TrabajoEstado? estado = null,
            [FromQuery] int? vehiculoId = null,
            [FromQuery] bool? datosIncompletos = null)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            PagedResponse<TrabajoDto> trabajos = await _trabajoService.ObtenerTodosAsync(
                tallerId.Value,
                paginacion,
                estado,
                vehiculoId,
                datosIncompletos);

            return Ok(ApiResponse<PagedResponse<TrabajoDto>>.Ok(trabajos, "Listado de trabajos recuperado correctamente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<TrabajoDto> resultado = await _trabajoService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    mensaje: resultado.Message ?? "Trabajo no encontrado."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Trabajo recuperado correctamente."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearTrabajoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            int? usuarioId = _userContextService.GetUsuarioId();

            ServiceResult<TrabajoDto> resultado = await _trabajoService.CrearAsync(tallerId.Value, usuarioId, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al crear trabajo."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<TrabajoDto>.Ok(resultado.Data, "Trabajo creado correctamente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarTrabajoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            int? usuarioId = _userContextService.GetUsuarioId();

            ServiceResult<TrabajoDto> resultado = await _trabajoService.ActualizarAsync(tallerId.Value, id, usuarioId, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar trabajo."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Trabajo actualizado correctamente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _trabajoService.EliminarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar trabajo."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Trabajo archivado correctamente."));
        }

        /// <summary>Facturar un trabajo cerrado (CERRADO → FACTURADO). Genera snapshot inmutable de factura.</summary>
        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPost("{id:int:min(1)}/facturar")]
        public async Task<IActionResult> Facturar(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<TrabajoDto> resultado = await _trabajoService.FacturarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.TRA_NO_FACTURABLE.ToString(),
                    mensaje: resultado.Message ?? "No se pudo facturar el trabajo."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Trabajo facturado correctamente. Factura generada."));
        }
    }
}
