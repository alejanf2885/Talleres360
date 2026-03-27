using Microsoft.EntityFrameworkCore;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Cache;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

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
            ArgumentNullException.ThrowIfNull(marcaRepository);
            ArgumentNullException.ThrowIfNull(cacheService);

            _marcaRepository = marcaRepository;
            _cacheService = cacheService;
        }

        public async Task<ServiceResult<MarcaVehiculoDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID debe ser mayor a 0"
                );
            }

            Marca? marca = await _marcaRepository.GetMarcaByIdAsync(id);

            if (marca == null)
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    $"No se encontró la marca con ID {id}"
                );
            }

            MarcaVehiculoDto marcaDto = MapearMarcaDto(marca);
            return ServiceResult<MarcaVehiculoDto>.Ok(marcaDto);
        }

        public async Task<ServiceResult<MarcaVehiculoDto>> GetByNombreAsync(int tallerId, string nombre)
        {
            if (tallerId <= 0)
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller debe ser mayor a 0"
                );
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El nombre no puede estar vacío"
                );
            }

            string nombreNormalizado = nombre.Trim();

            Marca? marca = await _marcaRepository.GetMarcaVisibleByNombreAsync(tallerId, nombreNormalizado);

            if (marca == null)
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    $"No se encontró la marca con nombre '{nombreNormalizado}'"
                );
            }

            MarcaVehiculoDto marcaDto = MapearMarcaDto(marca);
            return ServiceResult<MarcaVehiculoDto>.Ok(marcaDto);
        }

        public async Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId)
        {
            if (tallerId <= 0)
            {
                return ServiceResult<List<MarcaVehiculoDto>>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller debe ser mayor a 0"
                );
            }

            string claveCache = $"marcas_taller_{tallerId}";

            List<MarcaVehiculoDto>? marcas = await _cacheService.GetOrSetAsync(
                claveCache,
                () => _marcaRepository.ObtenerMarcasAsync(tallerId),
                TimeSpan.FromMinutes(30)
            );

            List<MarcaVehiculoDto> marcasSeguras = marcas ?? new List<MarcaVehiculoDto>();
            return ServiceResult<List<MarcaVehiculoDto>>.Ok(marcasSeguras);
        }

        public async Task<ServiceResult<MarcaVehiculoDto>> RegistrarMarcaAsync(int tallerId, string nombre, bool esOficial)
        {
            if (tallerId <= 0)
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller debe ser mayor a 0"
                );
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El nombre de la marca no puede estar vacío"
                );
            }

            string nombreNormalizado = nombre.Trim();

            if (esOficial)
            {
                bool existeOficial = await _marcaRepository.ExisteMarcaOficialAsync(nombreNormalizado);
                if (existeOficial)
                {
                    return ServiceResult<MarcaVehiculoDto>.Fail(
                        ErrorCode.MAR_NOMBRE_DUPLICADO.ToString(),
                        $"Ya existe una marca oficial con el nombre '{nombreNormalizado}'"
                    );
                }
            }
            else
            {
                bool existeEnTaller = await _marcaRepository.ExisteMarcaEnTallerAsync(nombreNormalizado, tallerId);
                if (existeEnTaller)
                {
                    return ServiceResult<MarcaVehiculoDto>.Fail(
                        ErrorCode.MAR_NOMBRE_DUPLICADO.ToString(),
                        $"Ya existe una marca en el taller con el nombre '{nombreNormalizado}'"
                    );
                }
            }

            Marca marca = new Marca
            {
                Nombre = nombreNormalizado,
                EsOficial = esOficial,
                TallerId = esOficial ? null : tallerId
            };

            await _marcaRepository.AddAsync(marca);

            MarcaVehiculoDto marcaDto = MapearMarcaDto(marca);
            return ServiceResult<MarcaVehiculoDto>.Ok(marcaDto);
        }

        public async Task<ServiceResult<bool>> EliminarMarcaAsync(int tallerId, int marcaId)
        {
            if (tallerId <= 0 || marcaId <= 0)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El ID del taller y de la marca deben ser mayores a 0");
            }

            Marca? marca = await _marcaRepository.GetMarcaByIdAsync(marcaId);
            if (marca == null)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    $"No se encontró la marca con ID {marcaId}");
            }

            if (marca.EsOficial)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "No se puede eliminar una marca oficial.");
            }

            if (marca.TallerId != tallerId)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    "La marca no pertenece al taller.");
            }

            bool tieneDependencias = await _marcaRepository.TieneDependenciasAsync(marcaId);
            if (tieneDependencias)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "No se puede eliminar la marca porque tiene elementos asociados.");
            }

            try
            {
                await _marcaRepository.DeleteAsync(marca);
            }
            catch (DbUpdateException)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "No se pudo eliminar la marca por conflicto de integridad.");
            }

            Marca? verificacion = await _marcaRepository.GetMarcaByIdAsync(marcaId);
            if (verificacion != null)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "La marca no se eliminó correctamente.");
            }

            _cacheService.Remove($"marcas_taller_{tallerId}");
            return ServiceResult<bool>.Ok(true);
        }

        private static MarcaVehiculoDto MapearMarcaDto(Marca marca)
        {
            ArgumentNullException.ThrowIfNull(marca);

            MarcaVehiculoDto marcaDto = new MarcaVehiculoDto
            {
                Id = marca.Id,
                Nombre = marca.Nombre,
                EsOficial = marca.EsOficial
            };

            return marcaDto;
        }
    }
}