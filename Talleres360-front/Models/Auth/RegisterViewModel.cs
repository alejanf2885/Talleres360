using System.ComponentModel.DataAnnotations;
using Talleres360.Dtos.Auth;

namespace Talleres360_front.Models.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre del taller es obligatorio")]
    [StringLength(100)]
    public string NombreTaller { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del administrador es obligatorio")]
    [StringLength(100)]
    public string NombreAdmin { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula y un número")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public RegistroRequest ToRequest() => new()
    {
        NombreTaller = NombreTaller,
        NombreAdmin  = NombreAdmin,
        Email        = Email,
        Password     = Password
    };
}
