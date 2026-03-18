using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos.Emails;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Auth;
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
        private readonly INotificacionService _notificacionService; // ✅ Nueva abstracción

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
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new ApiErrorResponse(AuthErrorCode.TOKEN_INVALIDO.ToString(), "El token es obligatorio."));
            }

            var resultadoToken = await _verificacionService.ValidarYConsumirTokenAsync(token);
            if (!resultadoToken.Exito)
            {
                return BadRequest(new ApiErrorResponse(AuthErrorCode.TOKEN_INVALIDO.ToString(), resultadoToken.Mensaje));
            }

            await _usuarioService.ActivarUsuarioAsync(resultadoToken.UsuarioId.Value);

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
                return BadRequest(new ApiErrorResponse(AuthErrorCode.ERROR_GENERICO.ToString(), "Esta cuenta ya está activa."));
            }

            string token = await _verificacionService.GenerarTokenRegistroAsync(usuario.Id);

            ServiceResult<bool> resultadoEnvio = await _notificacionService.EnviarBienvenidaAsync(usuario, token);

            if (!resultadoEnvio.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse(
                    resultadoEnvio.ErrorCode ?? AuthErrorCode.ERROR_GENERICO.ToString(),
                    resultadoEnvio.Message ?? "Error al enviar el correo."
                ));
            }

            return Ok(new { Mensaje = "Se ha enviado un nuevo enlace de activación." });
        }
    }
}