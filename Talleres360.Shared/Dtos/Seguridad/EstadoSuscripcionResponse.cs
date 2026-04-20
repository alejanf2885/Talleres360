namespace Talleres360.Dtos.Seguridad
{
    public class EstadoSuscripcionResponse
    {
        public bool EsActivo { get; set; }
        public string TipoSuscripcion { get; set; } = string.Empty;
        public int DiasRestantesTrial { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}