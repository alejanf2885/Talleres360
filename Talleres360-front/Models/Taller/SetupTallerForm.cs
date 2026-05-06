using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Taller;

public class SetupTallerForm
{
    [Required(ErrorMessage = "El CIF es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string CIF { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(255, ErrorMessage = "Máximo 255 caracteres")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La localidad es obligatoria")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Localidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
    public string Telefono { get; set; } = string.Empty;

    public string? LogoBase64 { get; set; }
}
