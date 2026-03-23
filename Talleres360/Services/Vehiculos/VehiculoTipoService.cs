using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Services.Vehiculos
{
    public class VehiculoTipoService : IVehiculoTipoService
    {
        private readonly IVehiculoTipoRepository _vehiculoTipoRepository;
        private readonly ICacheService _cacheService;

        public VehiculoTipoService(
            IVehiculoTipoRepository vehiculoTipoRepository,
            ICacheService cacheService)
        {
            _vehiculoTipoRepository = vehiculoTipoRepository;
            _cacheService = cacheService;
        }

        public async Task<ServiceResult<List<VehiculoTipoDto>>> ObtenerTiposVehiculoAsync()
        {
            string claveCache = "vehiculo_tipos_oficiales";
            List<VehiculoTipoDto> tipos = await _cacheService.GetOrSetAsync(
                claveCache,
                async () => await _vehiculoTipoRepository.ObtenerTiposVehiculoAsync(),
                TimeSpan.FromHours(24));

            return ServiceResult<List<VehiculoTipoDto>>.Ok(tipos);
        }
    }
}
