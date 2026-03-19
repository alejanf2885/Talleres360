using Talleres360.Dtos.Usuarios;

namespace Talleres360.Dtos.Responses
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioLoginDto Usuario { get; set; } = null;
    }
}
