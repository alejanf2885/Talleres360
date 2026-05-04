using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Presupuestos;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PresupuestosController : ControllerBase
    {
        private readonly IPresupuestoService _presupuestoService;
        private readonly IUserContextService _userContextService;

        public PresupuestosController(IPresupuestoService presupuestoService, IUserContextService userContextService)
        {
            _presupuestoService  = presupuestoService;
            _userContextService  = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginacion)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            PagedResponse<TrabajoDto> presupuestos = await _presupuestoService.ObtenerTodosAsync(tallerId.Value, paginacion);
            return Ok(ApiResponse<PagedResponse<TrabajoDto>>.Ok(presupuestos, "Listado de presupuestos recuperado correctamente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            ServiceResult<TrabajoDto> resultado = await _presupuestoService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Presupuesto no encontrado."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Presupuesto recuperado correctamente."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearTrabajoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            int? usuarioId = _userContextService.GetUsuarioId();

            ServiceResult<TrabajoDto> resultado = await _presupuestoService.CrearAsync(tallerId.Value, usuarioId, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo crear el presupuesto."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<TrabajoDto>.Ok(resultado.Data, "Presupuesto creado correctamente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarTrabajoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            int? usuarioId = _userContextService.GetUsuarioId();

            ServiceResult<TrabajoDto> resultado = await _presupuestoService.ActualizarAsync(tallerId.Value, id, usuarioId, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar presupuesto."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Presupuesto actualizado correctamente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            ServiceResult<bool> resultado = await _presupuestoService.EliminarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar el presupuesto."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Presupuesto cancelado correctamente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPost("{id:int:min(1)}/enviar")]
        public async Task<IActionResult> Enviar(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            ServiceResult<TrabajoDto> resultado = await _presupuestoService.EnviarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
                    mensaje: resultado.Message ?? "No se pudo enviar el presupuesto."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Presupuesto enviado al cliente."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPost("{id:int:min(1)}/aceptar")]
        public async Task<IActionResult> Aceptar(int id, [FromBody] AceptarPresupuestoRequest? request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            ServiceResult<TrabajoDto> resultado = await _presupuestoService.AceptarAsync(tallerId.Value, id, request?.FirmaAceptacionUrl);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
                    mensaje: resultado.Message ?? "No se pudo aceptar el presupuesto."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Presupuesto aceptado. Trabajo abierto."));
        }

        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPost("{id:int:min(1)}/rechazar")]
        public async Task<IActionResult> Rechazar(int id, [FromBody] RechazarPresupuestoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
                return Unauthorized();

            ServiceResult<TrabajoDto> resultado = await _presupuestoService.RechazarAsync(tallerId.Value, id, request.MotivoRechazo);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
                    mensaje: resultado.Message ?? "No se pudo rechazar el presupuesto."));
            }

            return Ok(ApiResponse<TrabajoDto>.Ok(resultado.Data!, "Presupuesto rechazado."));
        }
    }
}
