using Talleres360.Models;

namespace Talleres360.Interfaces.Seguridad
{
    public interface ITokenService
    {
        string GenerarJwtToken(Usuario usuario);
    }
}
