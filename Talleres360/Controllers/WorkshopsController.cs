using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Talleres;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;

namespace Talleres360.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopsController : ControllerBase
    {
        private readonly ITallerService _tallerService;
        private readonly IUserContextService _userContext;

        public WorkshopsController(
            ITallerService tallerService,
            IUserContextService userContext)
        {
            _tallerService = tallerService;
            _userContext = userContext;
        }

        [HttpGet("my-workshop")]
        public async Task<IActionResult> GetMyWorkshop()
        {
            int? tallerId = _userContext.GetTallerId();

            if (tallerId == null)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_NO_AUTORIZADO.ToString(),
                    mensaje: "No se pudo identificar el taller desde el token de acceso."
                ));
            }

            ServiceResult<WorkshopDto> resultado = await _tallerService.ObtenerTallerPorIdAsync(tallerId.Value);

            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Taller no encontrado."
                ));
            }

            return Ok(ApiResponse<WorkshopDto>.Ok(resultado.Data!, "Datos del taller recuperados."));
        }

        [HttpPut("config")]
        public async Task<IActionResult> ConfigurarTaller([FromBody] ConfigurarTallerRequest request)
        {
            int? tallerId = _userContext.GetTallerId();

            if (tallerId == null)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_NO_AUTORIZADO.ToString(),
                    mensaje: "No se pudo identificar el taller desde el token de acceso."
                ));
            }

            ServiceResult<bool> resultado = await _tallerService.ConfigurarPerfilAsync(tallerId.Value, request);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar los datos del taller."
                ));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Perfil del taller configurado correctamente."));
        }
    }
}