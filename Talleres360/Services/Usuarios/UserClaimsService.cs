using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Talleres360.Interfaces.Usuarios;
using Talleres360.Models;

namespace Talleres360.Services.Usuarios
{
    public class UserClaimsService : IUserClaimsService
    {
        public ClaimsPrincipal CreatePrincipal(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("TallerId", usuario.TallerId.ToString()),
                new Claim("SecurityStamp", usuario.SecurityStamp ?? Guid.NewGuid().ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}