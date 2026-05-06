using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Trabajos;

public class LineaTrabajoFormModel
{
    /// <summary>
    /// ID del producto del inventario (null si es mano de obra o concepto libre).
    /// </summary>
    public int? ProductoId { get; set; }

    [Required(ErrorMessage = "El concepto es obligatorio")]
    [StringLength(255, ErrorMessage = "Máximo 255 caracteres")]
    public string Concepto { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; } = 1;

    [Required(ErrorMessage = "El precio unitario es obligatorio")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
    public decimal DescuentoPorcentaje { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100")]
    public decimal ImpuestoPorcentaje { get; set; } = 21;

    /// <summary>true si la línea representa mano de obra (horas de taller).</summary>
    public bool EsManoObra { get; set; } = false;

    /// <summary>Estado del material: PENDIENTE | SOLICITADO | RECIBIDO | MONTADO (nullable)</summary>
    public string? EstadoMaterial { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Las horas no pueden ser negativas")]
    public decimal? HorasAplicadas { get; set; }

    public decimal? PrecioHoraAplicado { get; set; }
}
