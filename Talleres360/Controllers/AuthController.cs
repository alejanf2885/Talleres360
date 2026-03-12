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

        // --- REGISTRO ---
        [HttpPost("register")]
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
        [EnableRateLimiting("LoginLimiter")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Usuario? usuario = await _authService.ValidarLoginAsync(request.Email, request.Password);

            if (usuario == null)
            {
                return Unauthorized(new { Error = "Credenciales incorrectas o cuenta inactiva." });
            }

            string jwtToken = _tokenService.GenerarJwtToken(usuario);

            string refreshToken = await _refreshTokenService.CrearRefreshTokenAsync(usuario.Id);

            return Ok(new
            {
                Token = jwtToken,
                RefreshToken = refreshToken, 
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
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { Error = "El Refresh Token es obligatorio." });
            }

            var resultado = await _refreshTokenService.ValidarYRenovarAsync(request.RefreshToken);

            if (!resultado.Exito)
            {
                // Si falla (caducado o robado), devuelve 401 y Angular cerrará la sesión
                return Unauthorized(new { Error = resultado.MensajeError });
            }

            return Ok(new
            {
                Token = resultado.NuevoJwtToken,
                RefreshToken = resultado.NuevoRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                // Quemamos el token en la base de datos para que nadie más pueda usarlo
                await _refreshTokenService.RevocarRefreshTokenAsync(request.RefreshToken);
            }

            return Ok(new { Mensaje = "Sesión cerrada correctamente." });
        }
    }
}