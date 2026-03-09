using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;
using Talleres360.Dtos.Auth;
using Talleres360.Dtos;

namespace Talleres360.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;

        public AuthController(IIdentityService identityService, ITokenService tokenService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
        }

        // --- REGISTRO ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroRequest request)
        {
            var (success, message) = await _identityService.RegistrarTallerNuevoAsync(
                request.NombreTaller,
                request.NombreAdmin,
                request.Email,
                request.Password);

            if (!success)
            {
                return BadRequest(new { Error = message });
            }

            return Ok(new { Mensaje = message });
        }

        // --- LOGIN ---
        [HttpPost("login")]
        [EnableRateLimiting("LoginLimiter")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Usuario? usuario = await _identityService.ValidarLoginAsync(request.Email, request.Password);

            if (usuario == null)
            {
                // 401 Unauthorized
                return Unauthorized(new { Error = "Credenciales incorrectas o cuenta inactiva." });
            }

            string token = _tokenService.GenerarJwtToken(usuario);

            return Ok(new
            {
                Token = token,
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

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok();
        }
    }

}