using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Presupuestos
{
    public class LineaPresupuestoRequest
    {
        public int? ServicioId { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(255, ErrorMessage = "El concepto no puede superar 255 caracteres")]
        public string Concepto { get; set; } = string.Empty;

        [Range(0.01, 99999999.99, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El precio unitario no es válido")]
        public decimal PrecioUnitario { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal DescuentoPorcentaje { get; set; }

        [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100")]
        public decimal ImpuestoPorcentaje { get; set; }
    }
}
