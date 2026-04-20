namespace Talleres360.Dtos.Talleres
{
    public class WorkshopDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? CIF { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Telefono { get; set; }
        public bool PerfilConfigurado { get; set; }
        public string? TipoSuscripcion { get; set; }

        public string Logo { get; set; } = string.Empty;
    }
}