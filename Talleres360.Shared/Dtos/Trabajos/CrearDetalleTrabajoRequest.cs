using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Trabajos
{
    public class CrearDetalleTrabajoRequest
    {
        public int? ProductoId { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(255, ErrorMessage = "El concepto no puede superar 255 caracteres")]
        public string Concepto { get; set; } = string.Empty;

        [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo")]
        public decimal PrecioUnitario { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DescuentoPorcentaje { get; set; }

        [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100")]
        public decimal ImpuestoPorcentaje { get; set; } = 21;

        public bool EsManoObra { get; set; } = false;

        public string? EstadoMaterial { get; set; }

        // Si EsManoObra = true y no se envía, se rellena automáticamente desde tarifa activa
        public decimal? PrecioHoraAplicado { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Las horas aplicadas no pueden ser negativas")]
        public decimal? HorasAplicadas { get; set; }
    }
}
