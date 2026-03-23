using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IVehiculoMaestroRepository
    {
        Task<List<VehiculoTipoDto>> ObtenerTiposVehiculoAsync();
        Task<List<MarcaVehiculoDto>> ObtenerMarcasAsync(int tallerId);
        Task<bool> ExisteMarcaVisibleAsync(int tallerId, int marcaId);
        Task<List<ModeloVehiculoDto>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId);
    }
}
