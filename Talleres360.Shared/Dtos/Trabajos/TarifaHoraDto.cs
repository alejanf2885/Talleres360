namespace Talleres360.Dtos.Trabajos
{
    public class TarifaHoraDto
    {
        public int Id { get; set; }
        public int TallerId { get; set; }
        public decimal PrecioHora { get; set; }
        public string? Descripcion { get; set; }
        public DateOnly FechaVigencia { get; set; }
        public bool Activa { get; set; }
        public int? CreadoPorId { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
