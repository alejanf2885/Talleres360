namespace Talleres360.Dtos.DocumentosComerciales
{
    public class DocumentoComercialInput
    {
        public int ClienteId { get; set; }
        public int? TrabajoId { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? MetodoPago { get; set; }
        public string? NotasLegales { get; set; }
        public List<LineaDocumentoComercialInput> Lineas { get; set; } = new List<LineaDocumentoComercialInput>();
    }
}
