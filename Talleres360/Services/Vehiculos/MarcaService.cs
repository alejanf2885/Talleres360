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

            var marcaDto = new MarcaVehiculoDto
            {
                Id = marca.Id,
                Nombre = marca.Nombre,
                EsOficial = marca.EsOficial,
            };

            return ServiceResult<MarcaVehiculoDto>.Ok(marcaDto);
        }

        public async Task<ServiceResult<MarcaVehiculoDto>> GetByNombreAsync(int tallerId, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El nombre no puede estar vacío"
                );
            }

            Marca? marca = await _marcaRepository.GetMarcaVisibleByNombreAsync(tallerId, nombre.Trim());

            if (marca == null)
            {
                return ServiceResult<MarcaVehiculoDto>.Fail(
                    ErrorCode.VEH_MARCA_NO_ENCONTRADA.ToString(),
                    $"No se encontró la marca con nombre '{nombre}'"
                );
            }

            var marcaDto = new MarcaVehiculoDto
            {
                Id = marca.Id,
                Nombre = marca.Nombre,
                EsOficial = marca.EsOficial,
            };

            return ServiceResult<MarcaVehiculoDto>.Ok(marcaDto);
        }

        public async Task<ServiceResult<List<MarcaVehiculoDto>>> ObtenerMarcasAsync(int tallerId)
        {
            string claveCache = $"marcas_taller_{tallerId}";

            List<MarcaVehiculoDto> marcas = await _cacheService.GetOrSetAsync(
                claveCache,
                async () => await _marcaRepository.ObtenerMarcasAsync(tallerId),
                TimeSpan.FromMinutes(30)
            );

            return ServiceResult<List<MarcaVehiculoDto>>.Ok(marcas);
        }

        public async Task<ServiceResult<MarcaVehiculoDto>> RegistrarMarcaAsync(int tallerId, string nombre, bool esOficial)
        {
            // Validación de entrada
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
                        ErrorCode.VEH_MARCA_DUPLIC