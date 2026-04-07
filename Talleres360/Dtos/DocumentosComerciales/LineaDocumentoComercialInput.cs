namespace Talleres360.Dtos.DocumentosComerciales
{
    public class LineaDocumentoComercialInput
    {
        public int? ServicioId { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public decimal ImpuestoPorcentaje { get; set; }
    }
}
