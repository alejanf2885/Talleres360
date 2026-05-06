using System.ComponentModel.DataAnnotations;
using Talleres360.Enums;

namespace Talleres360_front.Models.Citas;

public class CitaFormModel
{
    public int Id { get; set; }

    public int? VehiculoId { get; set; }

    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string? NombreClienteTemp { get; set; }

    [Required(ErrorMessage = "La fecha de la cita es obligatoria")]
    public DateTime FechaCita { get; set; } = DateTime.Today.AddDays(1);

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string? HoraAproximada { get; set; }

    [StringLength(255, ErrorMessage = "Máximo 255 caracteres")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    public CitaEstado Estado { get; set; } = CitaEstado.PENDIENTE;
}
