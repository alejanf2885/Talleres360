namespace Talleres360.Dtos.Presupuestos
{
    public class PresupuestoDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int? TrabajoId { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ImporteImpuestos { get; set; }
        public decimal Total { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public string? NotasLegales { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteNifCif { get; set; } = string.Empty;
        public string? ClienteDireccion { get; set; }
        public string? ClienteCodigoPostal { get; set; }
        public string? ClienteLocalidad { get; set; }
        public string? ClienteProvincia { get; set; }
        public List<LineaPresupuestoDto> Lineas { get; set; } = new List<LineaPresupuestoDto>();
    }
}
