using System.ComponentModel.DataAnnotations;
using Talleres360.Enums;

namespace Talleres360_front.Models.Presupuestos;

public class PresupuestoFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El vehículo es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un vehículo válido")]
    public int VehiculoId { get; set; }

    public int? MecanicoAsignadoId { get; set; }

    public int? CitaId { get; set; }

    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string? TituloMantenimiento { get; set; }

    public string? TrabajoRealizado { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser 0 o mayor")]
    public int KmEntrada { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    public TrabajoEstado Estado { get; set; } = TrabajoEstado.PRESUPUESTO;

    [Required(ErrorMessage = "El estado de pago es obligatorio")]
    public TrabajoEstadoPago EstadoPago { get; set; } = TrabajoEstadoPago.PENDIENTE;

    [DataType(DataType.Date)]
    public DateTime? FechaEntregaEstimada { get; set; }
}
