namespace Talleres360.Dtos.Trabajos
{
    public class DetalleTrabajoDto
    {
        public int Id { get; set; }
        public int TrabajoId { get; set; }
        public int? ProductoId { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public decimal ImpuestoPorcentaje { get; set; }
        public bool EsManoObra { get; set; }
        public string? EstadoMaterial { get; set; }
        public int? TarifaHoraId { get; set; }
        public decimal? PrecioHoraAplicado { get; set; }
        public decimal? HorasAplicadas { get; set; }
    }
}
