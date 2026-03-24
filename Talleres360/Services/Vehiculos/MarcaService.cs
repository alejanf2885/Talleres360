using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Services.Vehiculos
{
    public class MarcaService : IMarcaService
    {
        private readonly IMarcaRepository _marcaRepository;
        private readonly ICacheService _cacheService;

        public MarcaService(
            IMarcaRepository marcaRepository,
            ICacheService cacheService)
        {
            _marcaRepository = marcaRepository;
            _cacheService = cacheService;
        }

        public async Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId)
        {
            string claveCache = $"marcas_taller_{tallerId}";
            List<MarcaVehiculoDto> marcas = await _cacheService.GetOrSetAsync(
                claveCache,
                async () => await _marcaRepository.ObtenerMarcasAsync(tallerId),
                TimeSpan.FromMinutes(30));

            return ServiceResult<List<MarcaVehiculoDto>>.Ok(marcas);
        }
    }
}
