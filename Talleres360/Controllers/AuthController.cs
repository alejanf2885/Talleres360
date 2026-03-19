using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models;
using Talleres360.Extensions;
using Talleres360.Dtos.Seguridad; 

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
            ServiceResult<bool> resultado = await _registroTallerService.RegistrarNuevoClienteSaaSAsync(request);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.REG_FALLIDO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo completar el registro."
                ));
            }

            return Ok(new { Mensaje = resultado.Message ?? "Registro completado con éxito." });
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthStrict")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            ServiceResult<Usuario> resultado = await _authService.ValidarLoginAsync(request.Email, request.Password);

            if (!resultado.Success)
            {
                int statusCode = resultado.ErrorCode == ErrorCode.AUTH_CUENTA_INACTIVA.ToString()
                                 ? StatusCodes.Status403Forbidden
                                 : StatusCodes.Status401Unauthorized;

                return StatusCode(statusCode, new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error de autenticación"
                ));
            }

            Usuario usuario = resultado.Data!;

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
            string? refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(),
                    mensaje: "No hay token de refresco."
                ));
            }

            ServiceResult<TokenResponseDto> resultado = await _refreshTokenService.ValidarYRenovarAsync(refreshToken);

            if (!resultado.Success)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(),
                    mensaje: resultado.Message ?? "La sesión ha expirado o no es válida."
                ));
            }

            Response.AppendRefreshTokenCookie(resultado.Data!.RefreshToken);

            return Ok(new { Token = resultado.Data!.Token });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? refreshToken = Request.Cookies["refreshToken"];

            Response.Cookies.Delete("refreshToken");

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                ServiceResult<bool> resultado = await _refreshTokenService.RevocarRefreshTokenAsync(refreshToken);

                if (!resultado.Success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse(
                        codigo: resultado.ErrorCode ?? ErrorCode.AUTH_LOGOUT_FALLIDO.ToString(),
                        mensaje: resultado.Message ?? "Ocurrió un error al intentar cerrar la sesión en el servidor."
                    ));
                }
            }

            return Ok(new { Mensaje = "Sesión cerrada correctamente." });
        }
    }
}