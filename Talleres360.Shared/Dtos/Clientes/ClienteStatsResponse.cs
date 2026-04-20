namespace Talleres360.Dtos.Clientes
{
    public class ClienteStatsResponse
    {
        public int TotalClientes { get; set; }
        public int ClientesNuevosEsteMes { get; set; }
        public int? LimitePlan { get; set; }
        public string NombrePlan { get; set; } = string.Empty;
    }
}
