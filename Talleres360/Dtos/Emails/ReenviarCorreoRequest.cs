using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Emails
{
    public class ReenviarCorreoRequest
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Email { get; set; }
    }
}
