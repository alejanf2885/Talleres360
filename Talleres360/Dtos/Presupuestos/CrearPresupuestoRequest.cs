using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Presupuestos
{
    public class CrearPresupuestoRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El cliente es obligatorio")]
        public int ClienteId { get; set; }

        public int? TrabajoId { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [StringLength(50, ErrorMessage = "El método de pago no puede superar 50 caracteres")]
        public string? MetodoPago { get; set; }

        public string? NotasLegales { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos una línea")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos una línea")]
        public List<LineaPresupuestoRequest> Lineas { get; set; } = new List<LineaPresupuestoRequest>();
    }
}
