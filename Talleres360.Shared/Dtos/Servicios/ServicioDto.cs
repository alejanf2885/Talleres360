namespace Talleres360.Dtos.Servicios
{
    public class ServicioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal ImpuestoPorcentaje { get; set; }
        public bool Activo { get; set; }
    }
}
