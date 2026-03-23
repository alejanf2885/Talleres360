using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IMarcaRepository
    {
        Task<List<MarcaVehiculoDto>> ObtenerMarcasAsync(int tallerId);
        Task<bool> ExisteMarcaVisibleAsync(int tallerId, int marcaId);
    }
}
