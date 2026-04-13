using Talleres360.Enums;

namespace Talleres360.Dtos.Trabajos
{
    public class CobroTrabajoDto
    {
        public int Id { get; set; }
        public int TrabajoId { get; set; }
        public decimal Importe { get; set; }
        public CobroMetodoPago? MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public DateTime FechaCobro { get; set; }
    }
}
