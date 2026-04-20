using System.ComponentModel.DataAnnotations;

namespace Talleres360.Notifications.Api.Dtos
{
    public class EnviarBienvenidaRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token es obligatorio")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La URL del frontend es obligatoria")]
        public string FrontendUrl { get; set; } = string.Empty;
    }
}
