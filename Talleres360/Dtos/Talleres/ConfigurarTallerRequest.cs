using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Talleres
{
    public class ConfigurarTallerRequest
    {
        [Required(ErrorMessage = "El CIF es obligatorio")]
        [MaxLength(20)]
        public string CIF { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [MaxLength(255)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La localidad es obligatoria")]
        [MaxLength(100)]
        public string Localidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El logo es obligatorio")]
        public IFormFile Logo { get; set; }
    }
}
