namespace Talleres360.Dtos.Seguridad
{
    public class TokenRefreshResult
    {
        public bool Exito { get; set; }

        public string MensajeError { get; set; } = string.Empty;

        public string NuevoJwtToken { get; set; } = string.Empty;
        public string NuevoRefreshToken { get; set; } = string.Empty;
    }
}
