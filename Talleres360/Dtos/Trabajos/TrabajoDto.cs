using Talleres360.Enums;

namespace Talleres360.Dtos.Trabajos
{
    public class TrabajoDto
    {
        public int Id { get; set; }
        public int? VehiculoId { get; set; }
        public int? MecanicoAsignadoId { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? TituloMantenimiento { get; set; }
        public string? TrabajoRealizado { get; set; }
        public int KmEntrada { get; set; }
        public TrabajoEstado Estado { get; set; }
        public TrabajoEstadoPago EstadoPago { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ImporteImpuestos { get; set; }
        public decimal Total { get; set; }
        public int? CreadoPorId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int? ModificadoPorId { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public bool DatosIncompletos { get; set; }
    }
}
