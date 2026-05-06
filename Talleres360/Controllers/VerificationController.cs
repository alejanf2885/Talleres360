using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos.Emails;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Emails;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IVerificacionService _verificacionService;
        private readonly INotificacionService _notificacionService;

        public VerificationController(
            IUsuarioService usuarioService,
            IVerificacionService verificacionService,
            INotificacionService notificacionService)
        {
            _usuarioService = usuarioService;
            _verificacionService = verificacionService;
            _notificacionService = notificacionService;
        }

        [HttpGet("verify-email")]
        [EnableRateLimiting("VerifyStrict")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new ApiErrorResponse(
                    ErrorCode.AUTH_TOKEN_INVALIDO.ToString(),
                    "El token es obligatorio."
                ));
            }

            ServiceResult<int> resultadoToken = await _verificacionService.ValidarYConsumirTokenAsync(token);

            if (!resultadoToken.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultadoToken.ErrorCode ?? ErrorCode.AUTH_TOKEN_INVALIDO.ToString(),
                    mensaje: resultadoToken.Message ?? "Error al validar el token."
                ));
            }

            ServiceResult<bool> resultadoActivacion = await _usuarioService.ActivarUsuarioAsync(resultadoToken.Data);

            if (!resultadoActivacion.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse(
                    codigo: resultadoActivacion.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultadoActivacion.Message ?? "Ocurri� un problema al activar la cuenta."
                ));
            }

            return Ok(ApiResponse<bool>.Ok(true, "�Cuenta verificada! Ya puedes iniciar sesi�n."));
        }

        [HttpPost("resend")]
        [EnableRateLimiting("EmailStrict")]
        public async Task<IActionResult> ResendVerification([FromBody] ReenviarCorreoRequest request)
        {
            ServiceResult<Usuario> resultadoUser = await _usuarioService.GetByEmailAsync(request.Email);

            if (!resultadoUser.Success)
            {
                // No revelar si el email existe (security by obscurity)
                return Ok(ApiResponse<bool>.Ok(true, "Si el correo existe, se ha enviado un enlace."));
            }

            Usuario usuario = resultadoUser.Data!;

            // Validar que no est� ya activo ANTES de generar token
            if (usuario.Activo)
            {
                return Ok(ApiResponse<bool>.Ok(true, "Si el correo existe, se ha enviado un enlace."));
            }

            string link = await _verificacionService.GenerarLinkVerificacionAsync(usuario.Id);
            ServiceResult<bool> resultadoEnvio = await _notificacionService.EnviarBienvenidaAsync(usuario, link);

            if (!resultadoEnvio.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse(
                    resultadoEnvio.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    resultadoEnvio.Message ?? "Error al enviar el correo."
                ));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Se ha enviado un nuevo enlace de activaci�n."));
        }
    }
}