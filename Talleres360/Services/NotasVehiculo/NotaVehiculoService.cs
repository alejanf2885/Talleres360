using Talleres360.Dtos.NotasVehiculo;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.NotasVehiculo;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Services.NotasVehiculo
{
    public class NotaVehiculoService : INotaVehiculoService
    {
        private readonly INotaVehiculoRepository _notaRepository;
        private readonly IVehiculoRepository _vehiculoRepository;

        public NotaVehiculoService(INotaVehiculoRepository notaRepository, IVehiculoRepository vehiculoRepository)
        {
            _notaRepository = notaRepository;
            _vehiculoRepository = vehiculoRepository;
        }

        public async Task<ServiceResult<List<NotaVehiculoDto>>> ObtenerPorVehiculoAsync(int tallerId, int vehiculoId)
        {
            bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(vehiculoId, tallerId);
            if (!vehiculoPertenece)
            {
                return ServiceResult<List<NotaVehiculoDto>>.Fail(
                    ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                    "Vehículo no encontrado.");
            }

            List<NotaVehiculoDto> notas = await _notaRepository.ObtenerPorVehiculoAsync(tallerId, vehiculoId);
            return ServiceResult<List<NotaVehiculoDto>>.Ok(notas);
        }

        public async Task<ServiceResult<NotaVehiculoDto>> ObtenerPorIdAsync(int tallerId, int notaId)
        {
            NotaVehiculo? entidad = await _notaRepository.ObtenerEntidadPorIdAsync(notaId);
            if (entidad == null || entidad.TallerId != tallerId || entidad.Eliminado)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Nota no encontrada.");
            }

            NotaVehiculoDto? detalle = await _notaRepository.ObtenerDetallePorIdAsync(notaId);
            if (detalle == null)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Nota no encontrada.");
            }

            return ServiceResult<NotaVehiculoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<NotaVehiculoDto>> CrearAsync(int tallerId, int vehiculoId, int? usuarioId, CrearNotaVehiculoRequest request)
        {
            if (!request.Tipo.HasValue)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El tipo de nota no es válido.");
            }

            bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(vehiculoId, tallerId);
            if (!vehiculoPertenece)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                    "Vehículo no encontrado.");
            }

            NotaVehiculo nota = new NotaVehiculo
            {
                TallerId = tallerId,
                VehiculoId = vehiculoId,
                UsuarioId = usuarioId,
                Texto = request.Texto.Trim(),
                Tipo = request.Tipo.Value,
                Resuelta = false,
                FechaCreacion = DateTime.UtcNow,
                FechaResolucion = null,
                Eliminado = false
            };

            await _notaRepository.AddAsync(nota);

            NotaVehiculoDto? detalle = await _notaRepository.ObtenerDetallePorIdAsync(nota.Id);
            if (detalle == null)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Nota creada pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<NotaVehiculoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<NotaVehiculoDto>> ActualizarAsync(int tallerId, int notaId, ActualizarNotaVehiculoRequest request)
        {
            NotaVehiculo? nota = await _notaRepository.ObtenerEntidadPorIdAsync(notaId);
            if (nota == null || nota.TallerId != tallerId || nota.Eliminado)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Nota no encontrada.");
            }

            if (!request.Tipo.HasValue)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El tipo de nota no es válido.");
            }

            nota.Texto = request.Texto.Trim();
            nota.Tipo = request.Tipo.Value;

            await _notaRepository.UpdateAsync(nota);

            NotaVehiculoDto? detalle = await _notaRepository.ObtenerDetallePorIdAsync(notaId);
            if (detalle == null)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Nota actualizada pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<NotaVehiculoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<NotaVehiculoDto>> ResolverAsync(int tallerId, int notaId)
        {
            NotaVehiculo? nota = await _notaRepository.ObtenerEntidadPorIdAsync(notaId);
            if (nota == null || nota.TallerId != tallerId || nota.Eliminado)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Nota no encontrada.");
            }

            nota.Resuelta = true;
            nota.FechaResolucion = DateTime.UtcNow;

            await _notaRepository.UpdateAsync(nota);

            NotaVehiculoDto? detalle = await _notaRepository.ObtenerDetallePorIdAsync(notaId);
            if (detalle == null)
            {
                return ServiceResult<NotaVehiculoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Nota resuelta pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<NotaVehiculoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<bool>> EliminarAsync(int tallerId, int notaId)
        {
            NotaVehiculo? nota = await _notaRepository.ObtenerEntidadPorIdAsync(notaId);
            if (nota == null || nota.TallerId != tallerId || nota.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Nota no encontrada.");
            }

            nota.Eliminado = true;
            await _notaRepository.UpdateAsync(nota);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
