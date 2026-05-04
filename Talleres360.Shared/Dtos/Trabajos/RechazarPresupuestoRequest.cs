using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Trabajos
{
    /// <summary>Request body para rechazar un presupuesto.</summary>
    public class RechazarPresupuestoRequest
    {
        /// <summary>Motivo del rechazo (obligatorio).</summary>
        [Required(ErrorMessage = "El motivo de rechazo es obligatorio.")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar 500 caracteres.")]
        public string MotivoRechazo { get; set; } = string.Empty;
    }
}
