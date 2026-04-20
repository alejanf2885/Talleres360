using System.ComponentModel.DataAnnotations;
using Talleres360.Enums;

namespace Talleres360.Dtos.Trabajos
{
    public class CrearCobroTrabajoRequest
    {
        [Required(ErrorMessage = "El importe es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a 0")]
        public decimal Importe { get; set; }

        public CobroMetodoPago? MetodoPago { get; set; }

        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string? Referencia { get; set; }

        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string? Notas { get; set; }

        [Required(ErrorMessage = "La fecha de cobro es obligatoria")]
        public DateTime FechaCobro { get; set; }
    }
}
