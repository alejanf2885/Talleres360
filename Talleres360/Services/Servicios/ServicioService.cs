using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Servicios;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Servicios;

namespace Talleres360.Services.Servicios
{
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _servicioRepository;

        public ServicioService(IServicioRepository servicioRepository)
        {
            _servicioRepository = servicioRepository;
        }

        public async Task<PagedResponse<ServicioDto>> ObtenerTodosAsync(int tallerId, PaginationParams paginacion, string? buscar = null, bool? activo = null)
        {
            PagedResponse<ServicioDto> servicios = await _servicioRepository.ObtenerTodosPagedAsync(tallerId, paginacion, buscar, activo);
            return servicios;
        }

        public async Task<ServiceResult<ServicioDto>> ObtenerPorIdAsync(int tallerId, int servicioId)
        {
            Servicio? entidad = await _servicioRepository.ObtenerEntidadPorIdAsync(servicioId);
            if (entidad == null || entidad.TallerId != tallerId || entidad.Eliminado)
            {
                return ServiceResult<ServicioDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Servicio no encontrado.");
            }

            ServicioDto? detalle = await _servicioRepository.ObtenerDetallePorIdAsync(servicioId);
            if (detalle == null)
            {
                return ServiceResult<ServicioDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Servicio no encontrado.");
            }

            return ServiceResult<ServicioDto>.Ok(detalle);
        }

        public async Task<ServiceResult<ServicioDto>> CrearAsync(int tallerId, CrearServicioRequest request)
        {
            bool existeNombre = await _servicioRepository.ExisteNombreAsync(tallerId, request.Nombre);
            if (existeNombre)
            {
                return ServiceResult<ServicioDto>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "Ya existe un servicio con ese nombre en el taller.");
            }

            Servicio servicio = new Servicio
            {
                TallerId = tallerId,
                Nombre = request.Nombre.Trim().ToUpper(),
                Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
                PrecioBase = request.PrecioBase,
                ImpuestoPorcentaje = request.ImpuestoPorcentaje,
                Activo = request.Activo,
                Eliminado = false
            };

            await _servicioRepository.AddAsync(servicio);

            ServicioDto? detalle = await _servicioRepository.ObtenerDetallePorIdAsync(servicio.Id);
            if (detalle == null)
            {
                return ServiceResult<ServicioDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Servicio creado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<ServicioDto>.Ok(detalle);
        }

        public async Task<ServiceResult<ServicioDto>> ActualizarAsync(int tallerId, int servicioId, ActualizarServicioRequest request)
        {
            Servicio? servicio = await _servicioRepository.ObtenerEntidadPorIdAsync(servicioId);
            if (servicio == null || servicio.TallerId != tallerId || servicio.Eliminado)
            {
                return ServiceResult<ServicioDto>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Servicio no encontrado.");
            }

            bool existeNombre = await _servicioRepository.ExisteNombreAsync(tallerId, request.Nombre, servicioId);
            if (existeNombre)
            {
                return ServiceResult<ServicioDto>.Fail(
                    ErrorCode.SYS_OPERACION_INVALIDA.ToString(),
                    "Ya existe otro servicio con ese nombre en el taller.");
            }

            servicio.Nombre = request.Nombre.Trim().ToUpper();
            servicio.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
            servicio.PrecioBase = request.PrecioBase;
            servicio.ImpuestoPorcentaje = request.ImpuestoPorcentaje;
            servicio.Activo = request.Activo;

            await _servicioRepository.UpdateAsync(servicio);

            ServicioDto? detalle = await _servicioRepository.ObtenerDetallePorIdAsync(servicioId);
            if (detalle == null)
            {
                return ServiceResult<ServicioDto>.Fail(
                    ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    "Servicio actualizado pero no se pudo recuperar su detalle.");
            }

            return ServiceResult<ServicioDto>.Ok(detalle);
        }

        public async Task<ServiceResult<bool>> EliminarAsync(int tallerId, int servicioId)
        {
            Servicio? servicio = await _servicioRepository.ObtenerEntidadPorIdAsync(servicioId);
            if (servicio == null || servicio.TallerId != tallerId || servicio.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Servicio no encontrado.");
            }

            servicio.Eliminado = true;
            await _servicioRepository.UpdateAsync(servicio);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
