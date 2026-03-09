using Talleres360.Models;
using System.Security.Claims;

namespace Talleres360.Interfaces.Usuarios
{
    public interface IUserClaimsService
    {
        ClaimsPrincipal CreatePrincipal(Usuario usuario);
    }
}
