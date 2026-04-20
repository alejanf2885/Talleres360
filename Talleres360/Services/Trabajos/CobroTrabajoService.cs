using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Services.Trabajos
{
    public class CobroTrabajoService : ICobroTrabajoService
    {
        private readonly ICobroTrabajoRepository _cobroRepository;
        private readonly ITrabajoRepository _trabajoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CobroTrabajoService(
            ICobroTrabajoRepository cobroRepository,
            ITrabajoRepository trabajoRepository,
            IUnitOfWork unitOfWork)
        {
            _cobroRepository   = cobroRepository;
            _trabajoRepository = trabajoRepository;
            _unitOfWork        = unitOfWork;
        }

        public async Task<PagedResponse<CobroTrabajoDto>> ObtenerTodosPorTrabajoAsync(int tallerId, int trabajoId, PaginationParams paginacion)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return new PagedResponse<CobroTrabajoDto>
                {
                    Data       = new List<CobroTrabajoDto>(),
                    PageNumber = paginacion.PageNumber,
                    PageSize   = paginacion.PageSize,
                    TotalCount = 0
                };
            }

            return await _cobroRepository.ObtenerTodosPorTrabajoPagedAsync(trabajoId, paginacion);
        }

        public async Task<ServiceResult<CobroTrabajoDto>> CrearAsync(int tallerId, int trabajoId, int? usuarioId, CrearCobroTrabajoRequest request)
        {
            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId || trabajo.Eliminado)
            {
                return ServiceResult<CobroTrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Trabajo no encontrado.");
            }

            if (request.Importe <= 0)
            {
                return ServiceResult<CobroTrabajoDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El importe debe ser mayor a 0.");
            }

            CobroTrabajo cobro = new CobroTrabajo
            {
                TallerId     = tallerId,
                TrabajoId    = trabajoId,
                Importe      = request.Importe,
                MetodoPago   = request.MetodoPago,
                Referencia   = string.IsNullOrWhiteSpace(request.Referencia) ? null : request.Referencia.Trim(),
                Notas        = string.IsNullOrWhiteSpace(request.Notas) ? null : request.Notas.Trim(),
                FechaCobro   = request.FechaCobro.Date,
                CreadoPorId  = usuarioId,
                Eliminado    = false
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _cobroRepository.AddAsync(cobro);
                await RecalcularEstadoPagoAsync(trabajo);
                await _trabajoRepository.UpdateAsync(trabajo);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                // Log inner exception details for debugging
                string detalles = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<CobroTrabajoDto>.Fail(
                    ErrorCode.SYS_ERROR_BASE_DATOS.ToString(),
                    $"No se pudo registrar el cobro. Detalles: {detalles}");
            }

            return ServiceResult<CobroTrabajoDto>.Ok(new CobroTrabajoDto
            {
                Id         = cobro.Id,
                TrabajoId  = cobro.TrabajoId,
                Importe    = cobro.Importe,
                MetodoPago = cobro.MetodoPago,
                Referencia = cobro.Referencia,
                Notas      = cobro.Notas,
                FechaCobro = cobro.FechaCobro
            });
        }

        public async Task<ServiceResult<bool>> EliminarAsync(int tallerId, int cobroId)
        {
            CobroTrabajo? cobro = await _cobroRepository.ObtenerEntidadPorIdAsync(cobroId);
            if (cobro == null || cobro.TallerId != tallerId || cobro.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    "Cobro no encontrado.");
            }

            Trabajo? trabajo = await _trabajoRepository.ObtenerEntidadPorIdAsync(cobro.TrabajoId);
            if (trabajo == null || trabajo.Eliminado)
            {
                return ServiceResult<bool>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(),
                    "Trabajo no encontrado.");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                cobro.Eliminado = true;
                await _cobroRepository.UpdateAsync(cobro);
                await RecalcularEstadoPagoAsync(trabajo);
                await _trabajoRepository.UpdateAsync(trabajo);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                // Log inner exception details for debugging
                string detalles = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ERROR_BASE_DATOS.ToString(),
                    $"No se pudo eliminar el cobro. Detalles: {detalles}");
            }

            return ServiceResult<bool>.Ok(true);
        }

        private async Task RecalcularEstadoPagoAsync(Trabajo trabajo)
        {
            decimal totalCobrado = await _cobroRepository.ObtenerTotalCobradoAsync(trabajo.Id);

            if (totalCobrado == 0)
            {
                trabajo.EstadoPago = TrabajoEstadoPago.PENDIENTE;
            }
            else if (totalCobrado >= trabajo.Total)
            {
                trabajo.EstadoPago = TrabajoEstadoPago.PAGADO;
            }
            else
            {
                trabajo.EstadoPago = TrabajoEstadoPago.PARCIAL;
            }

            trabajo.ModificadoPorId = null;
            trabajo.FechaUltimaModificacion = DateTime.UtcNow;
        }
    }
}
