using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Auth;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Extensions;
using Talleres360.Dtos.Seguridad; // ✅ Asegúrate de importar el namespace donde creaste el Helper

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRegistroTallerService _registroTallerService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthController(
            IAuthService authService,
            IRegistroTallerService registroTallerService,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService)
        {
            _authService = authService;
            _registroTallerService = registroTallerService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("register")]
        [EnableRateLimiting("AuthStrict")]
        public async Task<IActionResult> Register([FromBody] RegistroRequest request)
        {
            var (success, message) = await _registroTallerService.RegistrarNuevoClienteSaaSAsync(
                request.NombreTaller,
                request.NombreAdmin,
                request.Email,
                request.Password,
                request.Imagen);

            if (!success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: AuthErrorCode.REGISTRO_FALLIDO.ToString(),
                    mensaje: message
                ));
            }
            return Ok(new { Mensaje = message });
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthStrict")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Usuario? usuario = await _authService.ValidarLoginAsync(request.Email, request.Password);

            if (usuario == null)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: AuthErrorCode.CREDENCIALES_INCORRECTAS.ToString(),
                    mensaje: "El correo o la contraseña no son correctos."
                ));
            }

            if (!usuario.Activo)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    codigo: AuthErrorCode.CUENTA_INACTIVA.ToString(),
                    mensaje: "Tu cuenta aún no está verificada. Revisa tu correo electrónico.",
                    detalles: new { Email = usuario.Email }
                ));
            }

            string jwtToken = _tokenService.GenerarJwtToken(usuario);
            string refreshToken = await _refreshTokenService.CrearRefreshTokenAsync(usuario.Id);

            Response.AppendRefreshTokenCookie(refreshToken);

            return Ok(new
            {
                Token = jwtToken,
                Usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Email,
                    Rol = usuario.Rol.ToString(),
                    usuario.TallerId
                }
            });
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("RefreshPolicy")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: AuthErrorCode.REFRESH_TOKEN_INVALIDO.ToString(),
                    mensaje: "No hay token de refresco."
                ));
            }

            TokenRefreshResult resultado = await _refreshTokenService.ValidarYRenovarAsync(refreshToken);

            if (!resultado.Exito)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: AuthErrorCode.REFRESH_TOKEN_EXPIRADO.ToString(),
                    mensaje: resultado.MensajeError ?? "La sesión ha expirado."
                ));
            }

            Response.AppendRefreshTokenCookie(resultado.NuevoRefreshToken);

            return Ok(new { Token = resultado.NuevoJwtToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _refreshTokenService.RevocarRefreshTokenAsync(refreshToken);
            }

            Response.Cookies.Delete("refreshToken");

            return Ok(new { Mensaje = "Sesión cerrada correctamente." });
        }
    }
}