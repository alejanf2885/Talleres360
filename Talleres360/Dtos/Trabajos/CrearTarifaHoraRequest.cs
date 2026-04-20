using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Trabajos
{
    public class CrearTarifaHoraRequest
    {
        [Required(ErrorMessage = "El precio por hora es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal PrecioHora { get; set; }

        [StringLength(150, ErrorMessage = "La descripción no puede superar 150 caracteres")]
        public string? Descripcion { get; set; }

        public DateOnly? FechaVigencia { get; set; }
    }
}
