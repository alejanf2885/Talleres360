using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IModeloRepository
    {
        Task<List<ModeloVehiculoDto>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId);
    }
}
