using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Presupuestos;

public class RechazarPresupuestoFormModel
{
    [Required(ErrorMessage = "El motivo de rechazo es obligatorio")]
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
    public string MotivoRechazo { get; set; } = string.Empty;
}
