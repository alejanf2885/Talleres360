using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Controllers
{
    [Route("api/v1/trabajos/{trabajoId:int:min(1)}/cobros")]
    [ApiController]
    [Authorize]
    public class CobrosTrabajoController : ControllerBase
    {
        private readonly ICobroTrabajoService _cobroService;
        private readonly IUserContextService _userContextService;

        public CobrosTrabajoController(
            ICobroTrabajoService cobroService,
            IUserContextService userContextService)
        {
            _cobroService       = cobroService;
            _userContextService = userContextService;
        }

        [HttpGet]
        [TallerAuthorize<ITrabajoRepository>]
        public async Task<IActionResult> GetAll(int trabajoId, [FromQuery] PaginationParams paginacion)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            PagedResponse<CobroTrabajoDto> cobros = await _cobroService.ObtenerTodosPorTrabajoAsync(
                tallerId.Value,
                trabajoId,
                paginacion);

            return Ok(ApiResponse<PagedResponse<CobroTrabajoDto>>.Ok(cobros, "Listado de cobros recuperado correctamente."));
        }

        [HttpPost]
        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create(int trabajoId, [FromBody] CrearCobroTrabajoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            int? usuarioId = _userContextService.GetUsuarioId();

            ServiceResult<CobroTrabajoDto> resultado = await _cobroService.CrearAsync(
                tallerId.Value,
                trabajoId,
                usuarioId,
                request);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al registrar cobro."));
            }

            return CreatedAtAction(
                nameof(GetAll),
                new { trabajoId = trabajoId },
                ApiResponse<CobroTrabajoDto>.Ok(resultado.Data!, "Cobro registrado correctamente."));
        }

        [HttpDelete("{id:int:min(1)}")]
        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Delete(int trabajoId, int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _cobroService.EliminarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar cobro."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Cobro eliminado correctamente."));
        }
    }
}
