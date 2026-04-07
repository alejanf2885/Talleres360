using Talleres360.Enums;

namespace Talleres360.Dtos.Citas
{
    public class CitaDto
    {
        public int Id { get; set; }
        public int? VehiculoId { get; set; }
        public string? NombreClienteTemp { get; set; }
        public DateTime FechaCita { get; set; }
        public string? HoraAproximada { get; set; }
        public string? Descripcion { get; set; }
        public CitaEstado Estado { get; set; }
    }
}
