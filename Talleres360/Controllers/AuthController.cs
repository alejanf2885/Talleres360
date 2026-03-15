using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Dtos;
using Talleres360.Dtos.Auth;
using Talleres360.Interfaces.Auth; 
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres; 
using Talleres360.Models;

namespace Talleres360.API.Controllers
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
                request.Password);

            if (!success) return BadRequest(new { Error = message });

            return Ok(new { Mensaje = message });
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthStrict")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Usuario? usuario = await _authService.ValidarLoginAsync(request.Email, request.Password);

            if (usuario == null)
            {
                return Unauthorized(new { Error = "Credenciales incorrectas." });
            }

            string jwtToken = _tokenService.GenerarJwtToken(usuario);
            string refreshToken = await _refreshTokenService.CrearRefreshTokenAsync(usuario.Id);

            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true, 
                Secure = true,   
                SameSite = SameSiteMode.Strict, 
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

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
                return Unauthorized(new { Error = "No hay token de refresco." });
            }

            var resultado = await _refreshTokenService.ValidarYRenovarAsync(refreshToken);

            if (!resultado.Exito)
            {
                return Unauthorized(new { Error = resultado.MensajeError });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", resultado.NuevoRefreshToken, cookieOptions);

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