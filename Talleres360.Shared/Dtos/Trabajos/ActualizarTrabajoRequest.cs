using System.ComponentModel.DataAnnotations;
using Talleres360.Enums;

namespace Talleres360.Dtos.Trabajos
{
    public class ActualizarTrabajoRequest
    {
        public int? VehiculoId { get; set; }

        public int? MecanicoAsignadoId { get; set; }

        [StringLength(50, ErrorMessage = "El n�mero de documento no puede superar 50 caracteres")]
        public string? NumeroDocumento { get; set; }

        [StringLength(150, ErrorMessage = "El t�tulo no puede superar 150 caracteres")]
        public string? TituloMantenimiento { get; set; }

        public string? TrabajoRealizado { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje de entrada no es v�lido")]
        public int KmEntrada { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public TrabajoEstado? Estado { get; set; }

        [Required(ErrorMessage = "El estado de pago es obligatorio")]
        public TrabajoEstadoPago? EstadoPago { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El subtotal no es v�lido")]
        public decimal Subtotal { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El importe de impuestos no es v�lido")]
        public decimal ImporteImpuestos { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El total no es v�lido")]
        public decimal Total { get; set; }

        public bool DatosIncompletos { get; set; }

        public DateTime? FechaEntregaEstimada { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje de salida no es válido")]
        public int? KmSalida { get; set; }

        [StringLength(1000, ErrorMessage = "Las observaciones de entrega no pueden superar 1000 caracteres")]
        public string? ObservacionesEntrega { get; set; }
    }
}
