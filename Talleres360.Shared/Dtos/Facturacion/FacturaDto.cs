using Talleres360.Enums;

namespace Talleres360.Dtos.Facturacion
{
    public class FacturaDto
    {
        public int Id { get; set; }
        public int TallerId { get; set; }
        public int? TrabajoId { get; set; }
        public int ClienteId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public TipoDocumentoComercial TipoDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ImporteImpuestos { get; set; }
        public decimal Total { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public string? UrlPdf { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteNifCif { get; set; } = string.Empty;
        public string? ClienteEmail { get; set; }
        public string? ClienteTelefono { get; set; }

        public string? TallerNombre { get; set; }
        public string? TallerCif { get; set; }

        public List<LineaFacturaDto> Lineas { get; set; } = new();
    }
}
