using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Modelo;
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
                    "La marca indicada no existe o no est� disponible para el taller.");
            }

            List<ModeloVehiculoDto> modelos = await _modeloRepository.ObtenerModelosPorMarcaAsync(tallerId, marcaId);
            return ServiceResult<List<ModeloVehiculoDto>>.Ok(modelos);
        }

        public async Task<ServiceResult<ModeloVehiculoDto>> CrearModeloAsync(int tallerId, CrearModeloVehiculoDto crearModeloDto, bool esOficial)
        {
            if (tallerId <= 0)
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller debe ser mayor a 0");
            }

            ArgumentNullException.ThrowIfNull(crearModeloDto);

            if (crearModeloDto.MarcaId <= 0 || crearModeloDto.VehiculoTipoId <= 0)
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "La marca y el tipo de veh�culo son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(crearModeloDto.Nombre))
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El nombre del modelo es obligatorio.");
            }

            bool existeMarcaVisible = await _marcaRepository.ExisteMarcaVisibleAsync(tallerId, crearModeloDto.MarcaId);
            if (!existeMarcaVisible)
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    "La marca indicada no existe o no est� disponible para el taller.");
            }

            string nombreNormalizado = crearModeloDto.Nombre.Trim();

            if (esOficial)
            {
                bool existeOficial = await _modeloRepository.ExisteModeloOficialAsync(crearModeloDto.MarcaId, nombreNormalizado);
                if (existeOficial)
                {
                    return ServiceResult<ModeloVehiculoDto>.Fail(
                        ErrorCode.MAR_NOMBRE_DUPLICADO.ToString(),
                        $"Ya existe un modelo oficial con el nombre '{nombreNormalizado}' para la marca indicada.");
                }
            }
            else
            {
                bool existeEnTaller = await _modeloRepository.ExisteModeloEnTallerAsync(crearModeloDto.MarcaId, nombreNormalizado, tallerId);
                if (existeEnTaller)
                {
                    return ServiceResult<ModeloVehiculoDto>.Fail(
                        ErrorCode.MAR_NOMBRE_DUPLICADO.ToString(),
                        $"Ya existe un modelo con el nombre '{nombreNormalizado}' para esta marca en el taller.");
                }
            }

            Modelo modelo = new Modelo
            {
                MarcaId = crearModeloDto.MarcaId,
                VehiculoTipoId = crearModeloDto.VehiculoTipoId,
                Nombre = nombreNormalizado,
                EsOficial = esOficial,
                TallerId = esOficial ? null : tallerId
            };

            await _modeloRepository.AddAsync(modelo);

            ModeloVehiculoDto modeloDto = new ModeloVehiculoDto
            {
                Id = modelo.Id,
                MarcaId = modelo.MarcaId,
                VehiculoTipoId = modelo.VehiculoTipoId,
                Nombre = modelo.Nombre,
                EsOficial = modelo.EsOficial
            };

            return ServiceResult<ModeloVehiculoDto>.Ok(modeloDto);
        }

        public async Task<ServiceResult<ModeloVehiculoDto>> ActualizarModeloAsync(int tallerId, int modeloId, ActualizarModeloVehiculoDto actualizarModeloDto)
        {
            if (tallerId <= 0 || modeloId <= 0)
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller y del modelo deben ser mayores a 0");
            }

            ArgumentNullException.ThrowIfNull(actualizarModeloDto);

            Modelo? modelo = await _modeloRepository.GetByIdAsync(modeloId, tallerId);
            if (modelo == null || modelo.EsOficial)
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.VEH_MODELO_NO_ENCONTRADA.ToString(),
                    $"No se encontr� el modelo o no pertenece al taller");
            }

            if (actualizarModeloDto.MarcaId <= 0 || actualizarModeloDto.VehiculoTipoId <= 0 || string.IsNullOrWhiteSpace(actualizarModeloDto.Nombre))
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "Marca, tipo y nombre del modelo son obligatorios.");
            }

            bool existeMarcaVisible = await _marcaRepository.ExisteMarcaVisibleAsync(tallerId, actualizarModeloDto.MarcaId);
            if (!existeMarcaVisible)
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    "La marca indicada no existe o no est� disponible para el taller.");
            }

            string nombreNormalizado = actualizarModeloDto.Nombre.Trim();
            bool existeDuplicado = await _modeloRepository.ExisteModeloEnTallerAsync(actualizarModeloDto.MarcaId, nombreNormalizado, tallerId);

            if (existeDuplicado && (modelo.MarcaId != actualizarModeloDto.MarcaId || !string.Equals(modelo.Nombre, nombreNormalizado, StringComparison.OrdinalIgnoreCase)))
            {
                return ServiceResult<ModeloVehiculoDto>.Fail(
                    ErrorCode.MAR_NOMBRE_DUPLICADO.ToString(),
                    $"Ya existe un modelo con el nombre '{nombreNormalizado}' para esta marca en el taller.");
            }

            modelo.MarcaId = actualizarModeloDto.MarcaId;
            modelo.VehiculoTipoId = actualizarModeloDto.VehiculoTipoId;
            modelo.Nombre = nombreNormalizado;

            await _modeloRepository.UpdateAsync(modelo);

            ModeloVehiculoDto modeloDto = new ModeloVehiculoDto
            {
                Id = modelo.Id,
                MarcaId = modelo.MarcaId,
                VehiculoTipoId = modelo.VehiculoTipoId,
                Nombre = modelo.Nombre,
                EsOficial = modelo.EsOficial
            };

            return ServiceResult<ModeloVehiculoDto>.Ok(modeloDto);
        }

        public async Task<ServiceResult<bool>> EliminarModeloAsync(int tallerId, int modeloId)
        {
            if (tallerId <= 0 || modeloId <= 0)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller y del modelo deben ser mayores a 0");
            }

            Modelo? modelo = await _modeloRepository.GetByIdAsync(modeloId, tallerId);
            if (modelo == null || modelo.EsOficial)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.VEH_MODELO_NO_ENCONTRADA.ToString(),
                    $"No se encontr� el modelo o no pertenece al taller");
            }

            bool tieneDependencias = await _modeloRepository.TieneDependenciasAsync(modeloId);
            if (tieneDependencias)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "No se puede eliminar el modelo porque tiene veh�culos asociados.");
            }

            await _modeloRepository.DeleteAsync(modelo);

            Modelo? verificacion = await _modeloRepository.GetByIdAsync(modeloId, tallerId);
            if (verificacion != null)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "El modelo no se elimin� correctamente.");
            }

            return ServiceResult<bool>.Ok(true);
        }
    }
}
