namespace Talleres360.Dtos.Usuarios
{
    public class UsuarioLoginDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? TallerId { get; set; }
        public string SecurityStamp { get; set; } = string.Empty;
        public bool PerfilConfigurado { get; set; }
    }
}
