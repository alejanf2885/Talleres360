using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Clientes
{
    public class ActualizarClienteRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? Apellidos { get; set; }

        public string? NifCif { get; set; }
        public bool EsEmpresa { get; set; }
        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;
        public bool AceptaComunicaciones { get; set; }
    }
}