using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Servicios;

public class ServicioFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio base es obligatorio")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio base no puede ser negativo")]
    public decimal PrecioBase { get; set; }

    [Required(ErrorMessage = "El porcentaje de impuesto es obligatorio")]
    [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
    public decimal ImpuestoPorcentaje { get; set; } = 21;

    public bool Activo { get; set; } = true;
}
