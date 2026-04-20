using System.ComponentModel.DataAnnotations;
using Talleres360.Enums;

namespace Talleres360.Dtos.Trabajos
{
    public class CrearTrabajoRequest
    {
        public int? VehiculoId { get; set; }

        public int? MecanicoAsignadoId { get; set; }

        [StringLength(50, ErrorMessage = "El número de documento no puede superar 50 caracteres")]
        public string? NumeroDocumento { get; set; }

        [StringLength(150, ErrorMessage = "El título no puede superar 150 caracteres")]
        public string? TituloMantenimiento { get; set; }

        public string? TrabajoRealizado { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje de entrada no es válido")]
        public int KmEntrada { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public TrabajoEstado? Estado { get; set; }

        [Required(ErrorMessage = "El estado de pago es obligatorio")]
        public TrabajoEstadoPago? EstadoPago { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El subtotal no es válido")]
        public decimal Subtotal { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El importe de impuestos no es válido")]
        public decimal ImporteImpuestos { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El total no es válido")]
        public decimal Total { get; set; }

        public bool DatosIncompletos { get; set; }
    }
}
