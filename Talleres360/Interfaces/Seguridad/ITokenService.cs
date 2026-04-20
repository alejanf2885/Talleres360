using Talleres360.Dtos.Usuarios;

namespace Talleres360.Interfaces.Seguridad
{
    public interface ITokenService
    {
        string GenerarJwtToken(UsuarioLoginDto usuario);
    }
}
