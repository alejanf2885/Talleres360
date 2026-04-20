using Talleres360.Enums;

namespace Talleres360.Dtos.NotasVehiculo
{
    public class NotaVehiculoDto
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public int? UsuarioId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public NotaVehiculoTipo Tipo { get; set; }
        public bool Resuelta { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaResolucion { get; set; }
    }
}
