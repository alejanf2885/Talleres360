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
    [Authorize] // 🛡️ Todo el controlador requiere un token JWT válido
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
            // 1. Identificamos al Taller a partir del JWT
            int? tallerId = _userContext.GetTallerId();

            if (tallerId == null)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_NO_AUTORIZADO.ToString(),
                    mensaje: "No se pudo identificar el taller desde el token de acceso."
                ));
            }

            // 2. Pedimos los datos al servicio (que nos devuelve el DTO en un ServiceResult)
            ServiceResult<WorkshopDto> resultado = await _tallerService.ObtenerTallerPorIdAsync(tallerId.Value);

            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Taller no encontrado."
                ));
            }

            // 3. Devolvemos los datos limpios
            return Ok(resultado.Data);
        }

        [HttpPut("config")]
        public async Task<IActionResult> ConfigurarTaller([FromBody] ConfigurarTallerRequest request)
        {
            // 1. Identificamos al Taller
            int? tallerId = _userContext.GetTallerId();

            if (tallerId == null)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_NO_AUTORIZADO.ToString(),
                    mensaje: "No se pudo identificar el taller desde el token de acceso."
                ));
            }

            // 2. Pasamos el objeto Request completo al servicio
            ServiceResult<bool> resultado = await _tallerService.ConfigurarPerfilAsync(tallerId.Value, request);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar los datos del taller."
                ));
            }

            // 3. Confirmación de éxito
            return Ok(new { Mensaje = "Perfil del taller configurado correctamente." });
        }
    }
}