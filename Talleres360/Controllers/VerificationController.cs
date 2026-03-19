using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos.Emails;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Emails;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

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
                    mensaje: resultadoActivacion.Message ?? "Ocurrió un problema al activar la cuenta."
                ));
            }

            return Ok(new { Mensaje = "¡Cuenta verificada! Ya puedes iniciar sesión." });
        }

        [HttpPost("resend")]
        [EnableRateLimiting("EmailStrict")]
        public async Task<IActionResult> ResendVerification([FromBody] ReenviarCorreoRequest request)
        {
            ServiceResult<Usuario> resultadoUser = await _usuarioService.GetByEmailAsync(request.Email);

            if (!resultadoUser.Success)
                return Ok(new { Mensaje = "Si el correo existe, se ha enviado un enlace." });

            Usuario usuario = resultadoUser.Data!;

            if (usuario.Activo)
            {
                return BadRequest(new ApiErrorResponse(
                    ErrorCode.AUTH_CUENTA_YA_ACTIVA.ToString(),
                    "Esta cuenta ya está activa."
                ));
            }

            string token = await _verificacionService.GenerarTokenRegistroAsync(usuario.Id);

            ServiceResult<bool> resultadoEnvio = await _notificacionService.EnviarBienvenidaAsync(usuario, token);

            if (!resultadoEnvio.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse(
                    resultadoEnvio.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    resultadoEnvio.Message ?? "Error al enviar el correo."
                ));
            }

            return Ok(new { Mensaje = "Se ha enviado un nuevo enlace de activación." });
        }
    }
}