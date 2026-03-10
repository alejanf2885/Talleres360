using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Clientes
{
    public class ActualizarClienteRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;
        public string? Apellidos { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool AceptaComunicaciones { get; set; }
    }
}
