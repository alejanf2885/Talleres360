using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Clientes;

public class ClienteFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Apellidos { get; set; }

    [StringLength(20)]
    public string? NifCif { get; set; }

    public bool EsEmpresa { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Direccion { get; set; }

    [StringLength(15)]
    public string? CodigoPostal { get; set; }

    [StringLength(150)]
    public string? Localidad { get; set; }

    [StringLength(150)]
    public string? Provincia { get; set; }

    public bool AceptaComunicaciones { get; set; }
}
