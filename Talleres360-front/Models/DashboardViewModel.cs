using Talleres360.Dtos.Citas;
using Talleres360.Dtos.Trabajos;

namespace Talleres360_front.Models;

public class DashboardViewModel
{
    public int OrdenesAbiertas { get; set; }
    public int CitasPendientes { get; set; }
    public int TotalClientes { get; set; }
    public int ClientesNuevosEsteMes { get; set; }
    public List<TrabajoDto> UltimasOrdenes { get; set; } = new();
    public List<CitaDto> ProximasCitas { get; set; } = new();
}
