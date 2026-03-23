using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Services.Vehiculos
{
    public class VehiculoMaestroService : IVehiculoMaestroService
    {
        private readonly IVehiculoMaestroRepository _vehiculoMaestroRepository;
        private readonly ICacheService _cacheService;

        public VehiculoMaestroService(
            IVehiculoMaestroRepository vehiculoMaestroRepository,
            ICacheService cacheService)
        {
            _vehiculoMaestroRepository = vehiculoMaestroRepository;
            _cacheService = cacheService;
        }

        public async Task<ServiceResult<List<VehiculoTipoDto>>> ObtenerTiposVehiculoAsync()
        {
            string claveCache = "vehiculo_tipos_oficiales";
            List<VehiculoTipoDto> tipos = await _cacheService.GetOrSetAsync(
                claveCache,
                async () => await _vehiculoMaestroRepository.ObtenerTiposVehiculoAsync(),
                TimeSpan.FromHours(24));

            return ServiceResult<List<VehiculoTipoDto>>.Ok(tipos);
        }

        public async Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId)
        {
            List<MarcaVehiculoDto> marcas = await _vehiculoMaestroRepository.ObtenerMarcasAsync(tallerId);
            return ServiceResult<List<MarcaVehiculoDto>>.Ok(marcas);
        }

        public async Task<ServiceResult<List<ModeloVehiculoDto>>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId)
        {
            bool existeMarcaVisible = await _vehiculoMaestroRepository.ExisteMarcaVisibleAsync(tallerId, marcaId);
            if (!existeMarcaVisible)
            {
                return ServiceResult<List<ModeloVehiculoDto>>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    "La marca indicada no existe o no está disponible para el taller.");
            }

            List<ModeloVehiculoDto> modelos = await _vehiculoMaestroRepository.ObtenerModelosPorMarcaAsync(tallerId, marcaId);
            return ServiceResult<List<ModeloVehiculoDto>>.Ok(modelos);
        }
    }
}
