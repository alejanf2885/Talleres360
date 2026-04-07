using System.ComponentModel.DataAnnotations;
using Talleres360.Enums;

namespace Talleres360.Dtos.Citas
{
    public class ActualizarCitaRequest
    {
        public int? VehiculoId { get; set; }

        [StringLength(100, ErrorMessage = "El nombre temporal no puede superar 100 caracteres")]
        public string? NombreClienteTemp { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es obligatoria")]
        public DateTime FechaCita { get; set; }

        [StringLength(50, ErrorMessage = "La hora aproximada no puede superar 50 caracteres")]
        public string? HoraAproximada { get; set; }

        [StringLength(255, ErrorMessage = "La descripción no puede superar 255 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public CitaEstado? Estado { get; set; }
    }
}
