using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Services.Trabajos
{
    public class TrabajoService : ITrabajoService
    {
        private readonly ITrabajoRepository _trabajoRepository;
        private readonly IVehiculoRepository _vehiculoRepository;

        public TrabajoService(ITrabajoRepository trabajoRepository, IVehiculoRepository vehiculoRepository)
        {
            _trabajoRepository  = trabajoRepository;
            _vehiculoRepository = vehiculoRepository;
        }

        public async Task<PagedResponse<TrabajoDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion, TrabajoEstado? estado = null, int? vehiculoId = null, bool? datosIncompletos = null)
        {
            int? vehiculoIdFiltrado = vehiculoId;

            if (vehiculoIdFiltrado.HasValue)
            {
                bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(vehiculoIdFiltrado.Value, tallerId);
                if (!vehiculoPertenece)
                {
                    vehiculoIdFiltrado = null;
                }
            }

            PagedResponse<TrabajoDto> trabajos = await _trabajoRepository.ObtenerTodosPagedAsync(tallerId, paginacion, estado, vehiculoIdFiltrado, datosIncompletos);
            return trabajos;
        }

        public async Task<ServiceResult<TrabajoDto>> ObtenerPorIdAsync(int tallerId, int trabajoId)
        {
            Trabajo? entidad = await _trabajoRepository.ObtenerEntidadPorIdAsync(trabajoId);
            if (entidad == null || entidad.TallerId != tallerId || entidad.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Trabajo no encontrado.");
            }

            TrabajoDto? trabajo = await _trabajoRepository.ObtenerDetallePorIdAsync(trabajoId);
            if (trabajo == null)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Trabajo no encontrado.");
            }

            return ServiceResult<TrabajoDto>.Ok(trabajo);
        }

        public async Task<ServiceResult<TrabajoDto>> CrearAsync(int tallerId, int? usuarioId, CrearTrabajoRequest request)
        {
            if (!request.Estado.HasValue)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_ESTADO_INVALIDO.ToString(),
                    "El estado del trabajo no es válido.");
            }

            if (!request.EstadoPago.HasValue)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_ESTADO_PAGO_INVALIDO.ToString(),
                    "El estado de pago del trabajo no es válido.");
            }

            TrabajoEstado estadoTrabajo = request.Estado.Value;
            TrabajoEstadoPago estadoPagoTrabajo = request.EstadoPago.Value;

            if (request.VehiculoId.HasValue)
            {
                bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(request.VehiculoId.Value, tallerId);
                if (!vehiculoPertenece)
                {
                    return ServiceResult<TrabajoDto>.Fail(
                        ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                        "El vehículo indicado no existe en el taller.");
                }
            }

            bool datosIncompletosFinal = request.DatosIncompletos || !request.VehiculoId.HasValue;

            Trabajo trabajo = new Trabajo
            {
                TallerId                = tallerId,
                VehiculoId              = request.VehiculoId,
                MecanicoAsignadoId      = request.MecanicoAsignadoId,
                NumeroDocumento         = string.IsNullOrWhiteSpace(request.NumeroDocumento) ? null : request.NumeroDocumento.Trim().ToUpper(),
                TituloMantenimiento     = string.IsNullOrWhiteSpace(request.TituloMantenimiento) ? null : request.TituloMantenimiento.Trim(),
                TrabajoRealizado        = string.IsNullOrWhiteSpace(request.TrabajoRealizado) ? null : request.TrabajoRealizado.Trim(),
                KmEntrada               = request.KmEntrada,
                Estado                  = estadoTrabajo,
                EstadoPago              = estadoPagoTrabajo,
                Subtotal                = request.Subtotal,
                ImporteImpuestos        = request.ImporteImpuestos,
                Total                   = request.Total,
                CreadoPorId             = usuarioId,
                FechaCreacion           = DateTime.UtcNow,
                ModificadoPorId         = null,
                FechaUltimaModificacion = null,
                Eliminado               = false,
                DatosIncompletos        = datosIncompletosFinal
            };

            await _trabajoRepository.AddAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(trabajo.Id);
            if (detalle == null)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Trabajo creado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<TrabajoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<TrabajoDto>> ActualizarAsync(int tallerId, int trabajoId, int? usuarioId, ActualizarTrabajoRequest request)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Trabajo no encontrado.");
            }

            if (!request.Estado.HasValue)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_ESTADO_INVALIDO.ToString(),
                    "El estado del trabajo no es válido.");
            }

            if (!request.EstadoPago.HasValue)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_ESTADO_PAGO_INVALIDO.ToString(),
                    "El estado de pago del trabajo no es válido.");
            }

            TrabajoEstado estadoTrabajo = request.Estado.Value;
            TrabajoEstadoPago estadoPagoTrabajo = request.EstadoPago.Value;

            if (request.VehiculoId.HasValue)
            {
                bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(request.VehiculoId.Value, tallerId);
                if (!vehiculoPertenece)
                {
                    return ServiceResult<TrabajoDto>.Fail(
                        ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                        "El vehículo indicado no existe en el taller.");
                }
            }

            bool datosIncompletosFinal = request.DatosIncompletos || !request.VehiculoId.HasValue;

            trabajo.VehiculoId              = request.VehiculoId;
            trabajo.MecanicoAsignadoId      = request.MecanicoAsignadoId;
            trabajo.NumeroDocumento         = string.IsNullOrWhiteSpace(request.NumeroDocumento) ? null : request.NumeroDocumento.Trim().ToUpper();
            trabajo.TituloMantenimiento     = string.IsNullOrWhiteSpace(request.TituloMantenimiento) ? null : request.TituloMantenimiento.Trim();
            trabajo.TrabajoRealizado        = string.IsNullOrWhiteSpace(request.TrabajoRealizado) ? null : request.TrabajoRealizado.Trim();
            trabajo.KmEntrada               = request.KmEntrada;
            trabajo.Estado                  = estadoTrabajo;
            trabajo.EstadoPago              = estadoPagoTrabajo;
            trabajo.Subtotal                = request.Subtotal;
            trabajo.ImporteImpuestos        = request.ImporteImpuestos;
            trabajo.Total                   = request.Total;
            trabajo.ModificadoPorId         = usuarioId;
            trabajo.FechaUltimaModificacion = DateTime.UtcNow;
            trabajo.DatosIncompletos        = datosIncompletosFinal;

            await _trabajoRepository.UpdateAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(trabajoId);
            if (detalle == null)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Trabajo actualizado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<TrabajoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<bool>> EliminarAsync(int tallerId, int trabajoId)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Trabajo no encontrado.");
            }

            trabajo.Eliminado = true;
            await _trabajoRepository.UpdateAsync(trabajo);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
