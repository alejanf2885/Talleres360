using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos
{
    public class RegistroRequest
    {
        [Required(ErrorMessage = "El nombre del taller es obligatorio")]
        public string NombreTaller { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del administrador es obligatorio")]
        public string NombreAdmin { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        // Imagen es opcional (nullable)
        public string? Imagen { get; set; }

    }
}
