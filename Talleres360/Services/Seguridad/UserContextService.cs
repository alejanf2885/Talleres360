using System.Security.Claims;
using Talleres360.Interfaces.Seguridad;

namespace Talleres360.Services.Seguridad
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetTallerId()
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst("TallerId")?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }

        public int? GetUsuarioId()
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }

        public string? GetEmail() => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? GetRol() => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
    }
}