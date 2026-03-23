using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Services.Vehiculos
{
    public class ModeloService : IModeloService
    {
        private readonly IModeloRepository _modeloRepository;
        private readonly IMarcaRepository _marcaRepository;

        public ModeloService(
            IModeloRepository modeloRepository,
            IMarcaRepository marcaRepository)
        {
            _modeloRepository = modeloRepository;
            _marcaRepository = marcaRepository;
        }

        public async Task<ServiceResult<List<ModeloVehiculoDto>>> ObtenerModelosPorMarcaAsync(int tallerId, int marcaId)
        {
            bool existeMarcaVisible = await _marcaRepository.ExisteMarcaVisibleAsync(tallerId, marcaId);
            if (!existeMarcaVisible)
            {
                return ServiceResult<List<ModeloVehiculoDto>>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    "La marca indicada no existe o no está disponible para el taller.");
            }

            List<ModeloVehiculoDto> modelos = await _modeloRepository.ObtenerModelosPorMarcaAsync(tallerId, marcaId);
            return ServiceResult<List<ModeloVehiculoDto>>.Ok(modelos);
        }
    }
}
