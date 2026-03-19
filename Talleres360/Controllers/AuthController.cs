using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Seguridad;
using Talleres360.Dtos.Usuarios;
using Talleres360.Enums.Errors;
using Talleres360.Extensions;
using Talleres360.Interfaces.Auth;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;

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
        private readonly IUserContextService _userContext;

        public AuthController(
            IAuthService authService,
            IRegistroTallerService registroTallerService,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService,
            IUserContextService userContext)
        {
            _authService = authService;
            _registroTallerService = registroTallerService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _userContext = userContext;
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
                    mensaje: resultado.Message ?? "No se pudo completar el registro del taller."
                ));
            }

            return Ok(ApiResponse<object>.Ok(
                new { Email = request.Email.ToLower().Trim() },
                "¡Registro casi listo! Te hemos enviado un enlace de activación a tu correo electrónico."
            ));
        }
        [HttpPost("login")]
        [EnableRateLimiting("AuthStrict")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            ServiceResult<UsuarioLoginDto> resultado = await _authService.ValidarLoginAsync(request.Email, request.Password);

            if (!resultado.Success)
            {
                int statusCode = resultado.ErrorCode == ErrorCode.AUTH_CUENTA_INACTIVA.ToString()
                                 ? StatusCodes.Status403Forbidden
                                 : StatusCodes.Status401Unauthorized;

                return StatusCode(statusCode, new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.AUTH_CREDENCIALES_INCORRECTAS.ToString(),
                    mensaje: resultado.Message ?? "Las credenciales introducidas no son válidas."
                ));
            }

            UsuarioLoginDto usuario = resultado.Data!;

            string jwtToken = _tokenService.GenerarJwtToken(usuario);
            string refreshToken = await _refreshTokenService.CrearRefreshTokenAsync(usuario.Id);

            Response.AppendRefreshTokenCookie(refreshToken);

          

            AuthResponseDto authResponse = new AuthResponseDto
            {
                Token = jwtToken,
                Usuario = usuario
            };

            return Ok(ApiResponse<object>.Ok(authResponse, $"¡Bienvenido de nuevo, {usuario.Nombre}!"));
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
                    mensaje: "No se ha encontrado una sesión activa."
                ));
            }

            ServiceResult<TokenResponseDto> resultado = await _refreshTokenService.ValidarYRenovarAsync(refreshToken);

            if (!resultado.Success)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.AUTH_REFRESH_TOKEN_INVALIDO.ToString(),
                    mensaje: resultado.Message ?? "La sesión ha expirado. Por favor, vuelva a identificarse."
                ));
            }

            Response.AppendRefreshTokenCookie(resultado.Data!.RefreshToken);

            return Ok(ApiResponse<object>.Ok(
                new { Token = resultado.Data!.Token },
                "Token renovado con éxito."
            ));
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
                        codigo: ErrorCode.AUTH_LOGOUT_FALLIDO.ToString(),
                        mensaje: "Error interno al finalizar la sesión."
                    ));
                }
            }

            return Ok(ApiResponse<bool>.Ok(true, "Has cerrado sesión correctamente. ¡Hasta pronto!"));
        }

        [HttpPost("logout-all")]
        [Authorize]
        [EnableRateLimiting("AuthStrict")]
        public async Task<IActionResult> LogoutAll()
        {
            int? userId = _userContext.GetUsuarioId();

            if (!userId.HasValue)
            {
                return Unauthorized(new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_TOKEN_INVALIDO.ToString(),
                    mensaje: "No se ha podido identificar al usuario."
                ));
            }

            ServiceResult<bool> resultado = await _refreshTokenService.RevocarTodosLosTokensAsync(userId.Value);

            if (!resultado.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse(
                    codigo: ErrorCode.AUTH_REVOCACION_FALLIDA.ToString(),
                    mensaje: "Ocurrió un problema al intentar cerrar todas las sesiones."
                ));
            }

            Response.Cookies.Delete("refreshToken");

            return Ok(ApiResponse<bool>.Ok(true, "Se han cerrado todas las sesiones activas en otros dispositivos."));
        }
    }
}