using Talleres360.Dtos.Vehiculos;

namespace Talleres360.Interfaces.Vehiculos
{
    public interface IVehiculoTipoRepository
    {
        Task<List<VehiculoTipoDto>> ObtenerTiposVehiculoAsync();
    }
}
