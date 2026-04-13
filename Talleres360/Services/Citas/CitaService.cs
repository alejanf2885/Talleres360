using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Citas;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.Services.Citas
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepository;
        private readonly ITrabajoRepository _trabajoRepository;
        private readonly IVehiculoRepository _vehiculoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CitaService(
            ICitaRepository citaRepository,
            ITrabajoRepository trabajoRepository,
            IVehiculoRepository vehiculoRepository,
            IUnitOfWork unitOfWork)
        {
            _citaRepository     = citaRepository;
            _trabajoRepository  = trabajoRepository;
            _vehiculoRepository = vehiculoRepository;
            _unitOfWork         = unitOfWork;
        }

        public async Task<PagedResponse<CitaDto>> ObtenerTodasAsync(int tallerId, PaginationParams paginacion, DateTime? fechaDesde = null, DateTime? fechaHasta = null, CitaEstado? estado = null, int? vehiculoId = null)
        {
            return await _citaRepository.ObtenerTodasPagedAsync(tallerId, paginacion, fechaDesde, fechaHasta, estado, vehiculoId);
        }

        public async Task<ServiceResult<CitaDto>> ObtenerPorIdAsync(int tallerId, int citaId)
        {
            CitaDto? cita = await _citaRepository.ObtenerDetallePorIdAsync(citaId);
            if (cita == null)
            {
                return ServiceResult<CitaDto>.Fail(
                    ErrorCode.CITA_NO_ENCONTRADA.ToString(),
                    "Cita no encontrada.");
            }

            return ServiceResult<CitaDto>.Ok(cita);
        }

        public async Task<ServiceResult<CitaDto>> CrearAsync(int tallerId, CrearCitaRequest request)
        {
            CitaEstado estadoCita = request.Estado!.Value;

            if (!request.VehiculoId.HasValue && string.IsNullOrWhiteSpace(request.NombreClienteTemp))
            {
                return ServiceResult<CitaDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "Debe indicar un vehículo o un nombre temporal de cliente.");
            }

            if (request.VehiculoId.HasValue)
            {
                bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(request.VehiculoId.Value, tallerId);
                if (!vehiculoPertenece)
                {
                    return ServiceResult<CitaDto>.Fail(
                        ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                        "El vehículo indicado no existe en el taller.");
                }
            }

            Cita cita = new Cita
            {
                TallerId          = tallerId,
                VehiculoId        = request.VehiculoId,
                NombreClienteTemp = string.IsNullOrWhiteSpace(request.NombreClienteTemp)
                    ? null
                    : request.NombreClienteTemp.Trim(),
                FechaCita      = request.FechaCita.Date,
                HoraAproximada = string.IsNullOrWhiteSpace(request.HoraAproximada)
                    ? null
                    : request.HoraAproximada.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(request.Descripcion)
                    ? null
                    : request.Descripcion.Trim(),
                Estado    = estadoCita,
                Eliminado = false
            };

            await _citaRepository.AddAsync(cita);

            CitaDto? detalle = await _citaRepository.ObtenerDetallePorIdAsync(cita.Id);
            if (detalle == null)
            {
                return ServiceResult<CitaDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Cita creada pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<CitaDto>.Ok(detalle);
        }

        public async Task<ServiceResult<CitaDto>> ActualizarAsync(int tallerId, int citaId, ActualizarCitaRequest request)
        {
            Cita? cita = await _citaRepository.ObtenerEntidadPorIdAsync(citaId);
            if (cita == null || cita.TallerId != tallerId || cita.Eliminado)
            {
                return ServiceResult<CitaDto>.Fail(
                    ErrorCode.CITA_NO_ENCONTRADA.ToString(),
                    "Cita no encontrada.");
            }

            CitaEstado estadoCita = request.Estado!.Value;

            if (!request.VehiculoId.HasValue && string.IsNullOrWhiteSpace(request.NombreClienteTemp))
            {
                return ServiceResult<CitaDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "Debe indicar un vehículo o un nombre temporal de cliente.");
            }

            if (request.VehiculoId.HasValue)
            {
                bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(request.VehiculoId.Value, tallerId);
                if (!vehiculoPertenece)
                {
                    return ServiceResult<CitaDto>.Fail(
                        ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                        "El vehículo indicado no existe en el taller.");
                }
            }

            cita.VehiculoId = request.VehiculoId;
            cita.NombreClienteTemp = string.IsNullOrWhiteSpace(request.NombreClienteTemp)
                ? null
                : request.NombreClienteTemp.Trim();
            cita.FechaCita = request.FechaCita.Date;
            cita.HoraAproximada = string.IsNullOrWhiteSpace(request.HoraAproximada)
                ? null
                : request.HoraAproximada.Trim();
            cita.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion)
                ? null
                : request.Descripcion.Trim();
            cita.Estado = estadoCita;

            await _citaRepository.UpdateAsync(cita);

            CitaDto? detalle = await _citaRepository.ObtenerDetallePorIdAsync(citaId);
            if (detalle == null)
            {
                return ServiceResult<CitaDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Cita actualizada pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<CitaDto>.Ok(detalle);
        }

        public async Task<ServiceResult<bool>> EliminarAsync(int tallerId, int citaId)
        {
            Cita? cita = await _citaRepository.ObtenerEntidadPorIdAsync(citaId);
            if (cita == null || cita.TallerId != tallerId || cita.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.CITA_NO_ENCONTRADA.ToString(),
                    "Cita no encontrada.");
            }

            cita.Eliminado = true;
            await _citaRepository.UpdateAsync(cita);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<CitaTrabajoDto>> ConvertirATrabajoAsync(int tallerId, int citaId, int? usuarioId, ConvertirCitaTrabajoRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Cita? cita = await _citaRepository.ObtenerEntidadPorIdAsync(citaId);
            if (cita == null || cita.TallerId != tallerId || cita.Eliminado)
            {
                return ServiceResult<CitaTrabajoDto>.Fail(
                    ErrorCode.CITA_NO_ENCONTRADA.ToString(),
                    "Cita no encontrada.");
            }

            if (cita.Estado == CitaEstado.CANCELADA || cita.Estado == CitaEstado.COMPLETADA)
            {
                return ServiceResult<CitaTrabajoDto>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "La cita no se puede convertir a trabajo en su estado actual.");
            }

            if (!cita.VehiculoId.HasValue)
            {
                return ServiceResult<CitaTrabajoDto>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "Debes completar el vehículo antes de convertir la cita a trabajo.");
            }

            bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(cita.VehiculoId.Value, tallerId);
            if (!vehiculoPertenece)
            {
                return ServiceResult<CitaTrabajoDto>.Fail(
                    ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                    "El vehículo de la cita no pertenece a tu taller.");
            }

            bool datosIncompletos = string.IsNullOrWhiteSpace(request.TituloMantenimiento);

            Trabajo trabajo = new Trabajo
            {
                TallerId                = tallerId,
                VehiculoId              = cita.VehiculoId,
                MecanicoAsignadoId      = request.MecanicoAsignadoId,
                NumeroDocumento         = string.IsNullOrWhiteSpace(request.NumeroDocumento) ? null : request.NumeroDocumento.Trim().ToUpper(),
                TituloMantenimiento     = string.IsNullOrWhiteSpace(request.TituloMantenimiento) ? null : request.TituloMantenimiento.Trim(),
                TrabajoRealizado        = string.IsNullOrWhiteSpace(cita.Descripcion) ? null : cita.Descripcion.Trim(),
                KmEntrada               = request.KmEntrada,
                Estado                  = TrabajoEstado.ABIERTO,
                EstadoPago              = TrabajoEstadoPago.PENDIENTE,
                Subtotal                = 0,
                ImporteImpuestos        = 0,
                Total                   = 0,
                CreadoPorId             = usuarioId,
                FechaCreacion           = DateTime.UtcNow,
                ModificadoPorId         = null,
                FechaUltimaModificacion = null,
                Eliminado               = false,
                DatosIncompletos        = datosIncompletos
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await _trabajoRepository.AddAsync(trabajo);
                cita.Estado = CitaEstado.COMPLETADA;
                await _citaRepository.UpdateAsync(cita);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult<CitaTrabajoDto>.Fail(
                    ErrorCode.SYS_ERROR_BASE_DATOS.ToString(),
                    "No se pudo completar la conversión de la cita a trabajo.");
            }

            return ServiceResult<CitaTrabajoDto>.Ok(new CitaTrabajoDto
            {
                CitaId           = citaId,
                TrabajoId        = trabajo.Id,
                DatosIncompletos = trabajo.DatosIncompletos
            });
        }
    }
}
