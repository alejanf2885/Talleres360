using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.API.Filters;
using Talleres360.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Controllers
{
    [Route("api/v1/tarifas-hora")]
    [ApiController]
    [Authorize]
    public class TarifasHoraController : ControllerBase
    {
        private readonly ITarifaHoraService _tarifaService;
        private readonly IUserContextService _userContextService;

        public TarifasHoraController(ITarifaHoraService tarifaService, IUserContextService userContextService)
        {
            _tarifaService      = tarifaService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistorial()
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<IEnumerable<TarifaHoraDto>> resultado = await _tarifaService.ObtenerHistorialAsync(tallerId.Value);
            return Ok(ApiResponse<IEnumerable<TarifaHoraDto>>.Ok(resultado.Data!, "Historial de tarifas recuperado correctamente."));
        }

        [HttpGet("activa")]
        public async Task<IActionResult> GetActiva()
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<TarifaHoraDto?> resultado = await _tarifaService.ObtenerActivaAsync(tallerId.Value);
            return Ok(ApiResponse<TarifaHoraDto?>.Ok(resultado.Data, "Tarifa activa recuperada correctamente."));
        }

        [HttpPost]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create([FromBody] CrearTarifaHoraRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            int? usuarioId = _userContextService.GetUsuarioId();
            if (!usuarioId.HasValue) return Unauthorized();

            ServiceResult<TarifaHoraDto> resultado = await _tarifaService.CrearAsync(tallerId.Value, usuarioId.Value, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al crear tarifa."));
            }

            return Ok(ApiResponse<TarifaHoraDto>.Ok(resultado.Data!, "Tarifa creada correctamente."));
        }
    }
}
