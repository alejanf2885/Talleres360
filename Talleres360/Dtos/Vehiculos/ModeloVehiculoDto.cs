namespace Talleres360.Dtos.Vehiculos
{
    public class ModeloVehiculoDto
    {
        public int Id { get; set; }
        public int MarcaId { get; set; }
        public int VehiculoTipoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsOficial { get; set; }
    }
}
