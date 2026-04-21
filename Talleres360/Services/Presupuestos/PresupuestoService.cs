using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Presupuestos;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Services.Presupuestos
{
    public class PresupuestoService : IPresupuestoService
    {
        private readonly ITrabajoRepository _trabajoRepository;
        private readonly IVehiculoRepository _vehiculoRepository;

        public PresupuestoService(
            ITrabajoRepository trabajoRepository,
            IVehiculoRepository vehiculoRepository)
        {
            _trabajoRepository  = trabajoRepository;
            _vehiculoRepository = vehiculoRepository;
        }

        public async Task<PagedResponse<TrabajoDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion)
        {
            return await _trabajoRepository.ObtenerPresupuestosPagedAsync(tallerId, paginacion);
        }

        public async Task<ServiceResult<TrabajoDto>> ObtenerPorIdAsync(int tallerId, int presupuestoId)
        {
            Trabajo? entidad = await _trabajoRepository.ObtenerEntidadPorIdAsync(presupuestoId);
            if (entidad == null || entidad.TallerId != tallerId || entidad.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Presupuesto no encontrado.");
            }

            if (!entidad.Estado.EsPresupuesto())
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "El documento solicitado no es un presupuesto.");
            }

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(presupuestoId);
            if (detalle == null)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Presupuesto no encontrado.");
            }

            return ServiceResult<TrabajoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<TrabajoDto>> CrearAsync(int tallerId, int? usuarioId, CrearTrabajoRequest request)
        {
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

            string numeroDocumento = await _trabajoRepository.GenerarNumeroDocumentoTrabajoAsync(tallerId);
            bool datosIncompletosFinal = request.DatosIncompletos || !request.VehiculoId.HasValue;

            Trabajo trabajo = new Trabajo
            {
                TallerId            = tallerId,
                VehiculoId          = request.VehiculoId,
                MecanicoAsignadoId  = request.MecanicoAsignadoId,
                NumeroDocumento     = numeroDocumento,
                TituloMantenimiento = string.IsNullOrWhiteSpace(request.TituloMantenimiento) ? null : request.TituloMantenimiento.Trim(),
                TrabajoRealizado    = string.IsNullOrWhiteSpace(request.TrabajoRealizado) ? null : request.TrabajoRealizado.Trim(),
                KmEntrada           = request.KmEntrada,
                Estado              = TrabajoEstado.PRESUPUESTO,
                EstadoPago          = TrabajoEstadoPago.PENDIENTE,
                Subtotal            = request.Subtotal,
                ImporteImpuestos    = request.ImporteImpuestos,
                Total               = request.Total,
                CreadoPorId         = usuarioId,
                FechaCreacion       = DateTime.UtcNow,
                Eliminado           = false,
                DatosIncompletos    = datosIncompletosFinal
            };

            await _trabajoRepository.AddAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(trabajo.Id);
            if (detalle == null)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Presupuesto creado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<TrabajoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<TrabajoDto>> ActualizarAsync(int tallerId, int presupuestoId, int? usuarioId, ActualizarTrabajoRequest request)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(presupuestoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Presupuesto no encontrado.");
            }

            if (!trabajo.Estado.PermiteEdicion())
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
                    "Solo se pueden editar presupuestos en estado PRESUPUESTO o PRESUPUESTO_ENVIADO.");
            }

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
            trabajo.TituloMantenimiento     = string.IsNullOrWhiteSpace(request.TituloMantenimiento) ? null : request.TituloMantenimiento.Trim();
            trabajo.TrabajoRealizado        = string.IsNullOrWhiteSpace(request.TrabajoRealizado) ? null : request.TrabajoRealizado.Trim();
            trabajo.KmEntrada               = request.KmEntrada;
            trabajo.Subtotal                = request.Subtotal;
            trabajo.ImporteImpuestos        = request.ImporteImpuestos;
            trabajo.Total                   = request.Total;
            trabajo.ModificadoPorId         = usuarioId;
            trabajo.FechaUltimaModificacion = DateTime.UtcNow;
            trabajo.DatosIncompletos        = datosIncompletosFinal;

            await _trabajoRepository.UpdateAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(presupuestoId);
            if (detalle == null)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Presupuesto actualizado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<TrabajoDto>.Ok(detalle);
        }

        public async Task<ServiceResult<bool>> EliminarAsync(int tallerId, int presupuestoId)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(presupuestoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Presupuesto no encontrado.");
            }

            if (!trabajo.Estado.EsPresupuesto())
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.TRA_ESTADO_INVALIDO.ToString(),
                    "Solo se pueden eliminar presupuestos.");
            }

            trabajo.Eliminado = true;
            trabajo.Estado = TrabajoEstado.CANCELADO;
            await _trabajoRepository.UpdateAsync(trabajo);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<TrabajoDto>> EnviarAsync(int tallerId, int presupuestoId)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(presupuestoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Presupuesto no encontrado.");
            }

            if (!trabajo.Estado.PuedeTransicionarA(TrabajoEstado.PRESUPUESTO_ENVIADO))
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
                    $"No se puede enviar un presupuesto en estado {trabajo.Estado}.");
            }

            trabajo.Estado = TrabajoEstado.PRESUPUESTO_ENVIADO;
            trabajo.FechaEnvioPresupuesto = DateTime.UtcNow;
            trabajo.ValidezHastaPresupuesto = DateTime.UtcNow.AddDays(30);
            trabajo.FechaUltimaModificacion = DateTime.UtcNow;

            await _trabajoRepository.UpdateAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(presupuestoId);
            return detalle != null
                ? ServiceResult<TrabajoDto>.Ok(detalle)
                : ServiceResult<TrabajoDto>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "Error al recuperar el presupuesto.");
        }

        public async Task<ServiceResult<TrabajoDto>> AceptarAsync(int tallerId, int presupuestoId, string? firmaAceptacionUrl)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(presupuestoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Presupuesto no encontrado.");
            }

            if (!trabajo.Estado.PuedeTransicionarA(TrabajoEstado.ABIERTO))
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
                    $"No se puede aceptar un presupuesto en estado {trabajo.Estado}.");
            }

            trabajo.Estado = TrabajoEstado.ABIERTO;
            trabajo.FechaAceptacionPresupuesto = DateTime.UtcNow;
            trabajo.FirmaAceptacionUrl = string.IsNullOrWhiteSpace(firmaAceptacionUrl) ? null : firmaAceptacionUrl.Trim();
            trabajo.FechaUltimaModificacion = DateTime.UtcNow;

            await _trabajoRepository.UpdateAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(presupuestoId);
            return detalle != null
                ? ServiceResult<TrabajoDto>.Ok(detalle)
                : ServiceResult<TrabajoDto>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "Error al recuperar el trabajo.");
        }

        public async Task<ServiceResult<TrabajoDto>> RechazarAsync(int tallerId, int presupuestoId, string motivoRechazo)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(presupuestoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Presupuesto no encontrado.");
            }

            if (!trabajo.Estado.PuedeTransicionarA(TrabajoEstado.RECHAZADO))
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.TRA_TRANSICION_INVALIDA.ToString(),
                    $"No se puede rechazar un presupuesto en estado {trabajo.Estado}.");
            }

            if (string.IsNullOrWhiteSpace(motivoRechazo))
            {
                return ServiceResult<TrabajoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El motivo de rechazo es obligatorio.");
            }

            trabajo.Estado = TrabajoEstado.RECHAZADO;
            trabajo.MotivoRechazo = motivoRechazo.Trim();
            trabajo.FechaRechazo = DateTime.UtcNow;
            trabajo.FechaUltimaModificacion = DateTime.UtcNow;

            await _trabajoRepository.UpdateAsync(trabajo);

            TrabajoDto? detalle = await _trabajoRepository.ObtenerDetallePorIdAsync(presupuestoId);
            return detalle != null
                ? ServiceResult<TrabajoDto>.Ok(detalle)
                : ServiceResult<TrabajoDto>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "Error al recuperar el presupuesto.");
        }
    }
}
