namespace Talleres360.Dtos.Presupuestos
{
    public class LineaPresupuestoDto
    {
        public int Id { get; set; }
        public int? ServicioId { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public decimal ImpuestoPorcentaje { get; set; }
        public decimal SubtotalLinea { get; set; }
        public decimal TotalLinea { get; set; }
    }
}
