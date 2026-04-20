using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Controllers
{
    [Route("api/v1/trabajos/{trabajoId:int:min(1)}/detalles")]
    [ApiController]
    [Authorize]
    public class DetallesTrabajoController : ControllerBase
    {
        private readonly IDetalleTrabajoService _detalleService;
        private readonly IUserContextService _userContextService;

        public DetallesTrabajoController(IDetalleTrabajoService detalleService, IUserContextService userContextService)
        {
            _detalleService     = detalleService;
            _userContextService = userContextService;
        }

        [HttpGet]
        [TallerAuthorize<ITrabajoRepository>]
        public async Task<IActionResult> GetAll(int trabajoId)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<IEnumerable<DetalleTrabajoDto>> resultado = await _detalleService.ObtenerPorTrabajoAsync(tallerId.Value, trabajoId);
            return Ok(ApiResponse<IEnumerable<DetalleTrabajoDto>>.Ok(resultado.Data!, "Líneas recuperadas correctamente."));
        }

        [HttpPost]
        [TallerAuthorize<ITrabajoRepository>]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create(int trabajoId, [FromBody] CrearDetalleTrabajoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<DetalleTrabajoDto> resultado = await _detalleService.AgregarAsync(tallerId.Value, trabajoId, request);
            if (!resultado.Success)
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al agregar línea."));

            return Ok(ApiResponse<DetalleTrabajoDto>.Ok(resultado.Data!, "Línea agregada correctamente."));
        }

        [HttpDelete("{id:int:min(1)}")]
        [TallerAuthorize<ITrabajoRepository>]
        public async Task<IActionResult> Delete(int trabajoId, int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<bool> resultado = await _detalleService.EliminarAsync(tallerId.Value, trabajoId, id);
            if (!resultado.Success)
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar línea."));

            return Ok(ApiResponse<bool>.Ok(true, "Línea eliminada correctamente."));
        }
    }
}
