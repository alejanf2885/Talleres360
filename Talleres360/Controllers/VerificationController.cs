using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos.Emails;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Auth;
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
        private readonly IEmailService _emailService;

        public VerificationController(
            IUsuarioService usuarioService,
            IVerificacionService verificacionService,
            IEmailService emailService)
        {
            _usuarioService = usuarioService;
            _verificacionService = verificacionService;
            _emailService = emailService;
        }


        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new ApiErrorResponse(
                    AuthErrorCode.TOKEN_INVALIDO.ToString(),
                    "El token es obligatorio."
                ));
            }

            var resultado = await _verificacionService.ValidarYConsumirTokenAsync(token);

            if (!resultado.Exito)
            {
                return BadRequest(new ApiErrorResponse(
                    AuthErrorCode.TOKEN_INVALIDO.ToString(),
                    resultado.Mensaje
                ));
            }

            await _usuarioService.ActivarUsuarioAsync(resultado.UsuarioId.Value);

            return Ok(new { Mensaje = "¡Cuenta verificada! Ya puedes iniciar sesión." });
        }


        [HttpPost("resend")]
        [EnableRateLimiting("EmailStrict")]
        public async Task<IActionResult> ResendVerification([FromBody] ReenviarCorreoRequest request)
        {
            // 1. Validaciones de entrada (El escudo)
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new ApiErrorResponse(AuthErrorCode.ERROR_GENERICO.ToString(), "Email requerido."));
            }

            // 2. Buscar usuario
            var usuario = await _usuarioService.GetByEmailAsync(request.Email);
            if (usuario == null) return Ok(new { Mensaje = "Si el correo existe, se ha enviado." });

            if (usuario.Activo)
            {
                return BadRequest(new ApiErrorResponse(AuthErrorCode.ERROR_GENERICO.ToString(), "Cuenta ya activa."));
            }

            // 3. Generar Link (El servicio ya sabe la URL del Frontend por IOptions)
            string link = await _verificacionService.GenerarLinkVerificacionAsync(usuario.Id);

            // 4. Enviar Email (El servicio ya sabe dónde está el HTML)
            await _emailService.EnviarEmailBienvenidaAsync(usuario.Email, usuario.Nombre, link);

            return Ok(new { Mensaje = "Se ha enviado un nuevo enlace de activación." });
        }
    }
}