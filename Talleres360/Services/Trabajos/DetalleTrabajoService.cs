using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Data;
using Talleres360.Interfaces.Trabajos;

namespace Talleres360.Services.Trabajos
{
    public class DetalleTrabajoService : IDetalleTrabajoService
    {
        private readonly IDetalleTrabajoRepository _detalleRepo;
        private readonly ITrabajoRepository _trabajoRepo;
        private readonly ITarifaHoraRepository _tarifaRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DetalleTrabajoService(
            IDetalleTrabajoRepository detalleRepo,
            ITrabajoRepository trabajoRepo,
            ITarifaHoraRepository tarifaRepo,
            IUnitOfWork unitOfWork)
        {
            _detalleRepo = detalleRepo;
            _trabajoRepo = trabajoRepo;
            _tarifaRepo  = tarifaRepo;
            _unitOfWork  = unitOfWork;
        }

        public async Task<ServiceResult<IEnumerable<DetalleTrabajoDto>>> ObtenerPorTrabajoAsync(int tallerId, int trabajoId)
        {
            Trabajo? trabajo = await _trabajoRepo.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId)
                return ServiceResult<IEnumerable<DetalleTrabajoDto>>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(), "Trabajo no encontrado.");

            var detalles = await _detalleRepo.ObtenerPorTrabajoAsync(trabajoId);
            return ServiceResult<IEnumerable<DetalleTrabajoDto>>.Ok(detalles);
        }

        public async Task<ServiceResult<DetalleTrabajoDto>> AgregarAsync(int tallerId, int trabajoId, CrearDetalleTrabajoRequest request)
        {
            Trabajo? trabajo = await _trabajoRepo.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId)
                return ServiceResult<DetalleTrabajoDto>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(), "Trabajo no encontrado.");

            int? tarifaHoraId = null;
            decimal? precioHoraAplicado = request.PrecioHoraAplicado;

            if (request.EsManoObra && precioHoraAplicado == null)
            {
                TarifaHora? tarifaActiva = await _tarifaRepo.ObtenerActivaAsync(tallerId);
                if (tarifaActiva != null)
                {
                    tarifaHoraId       = tarifaActiva.Id;
                    precioHoraAplicado = tarifaActiva.PrecioHora;
                }
            }
            else if (request.EsManoObra && precioHoraAplicado != null)
            {
                TarifaHora? tarifaActiva = await _tarifaRepo.ObtenerActivaAsync(tallerId);
                if (tarifaActiva != null)
                    tarifaHoraId = tarifaActiva.Id;
            }

            var detalle = new DetalleTrabajo
            {
                TrabajoId          = trabajoId,
                TallerId           = tallerId,
                ProductoId         = request.ProductoId,
                Concepto           = request.Concepto.Trim(),
                Cantidad           = request.EsManoObra && request.HorasAplicadas.HasValue
                                        ? request.HorasAplicadas.Value
                                        : request.Cantidad,
                PrecioUnitario     = request.EsManoObra && precioHoraAplicado.HasValue
                                        ? precioHoraAplicado.Value
                                        : request.PrecioUnitario,
                DescuentoPorcentaje = request.DescuentoPorcentaje,
                ImpuestoPorcentaje = request.ImpuestoPorcentaje,
                EsManoObra         = request.EsManoObra,
                EstadoMaterial     = request.EstadoMaterial,
                TarifaHoraId       = tarifaHoraId,
                PrecioHoraAplicado = precioHoraAplicado,
                HorasAplicadas     = request.HorasAplicadas,
                Eliminado          = false
            };

            await _detalleRepo.AddAsync(detalle);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<DetalleTrabajoDto>.Ok(MapToDto(detalle));
        }

        public async Task<ServiceResult<bool>> EliminarAsync(int tallerId, int trabajoId, int detalleId)
        {
            Trabajo? trabajo = await _trabajoRepo.ObtenerEntidadPorIdAsync(trabajoId);
            if (trabajo == null || trabajo.TallerId != tallerId)
                return ServiceResult<bool>.Fail(
                    ErrorCode.TRA_NO_ENCONTRADO.ToString(), "Trabajo no encontrado.");

            DetalleTrabajo? detalle = await _detalleRepo.GetByIdAsync(detalleId, trabajoId);
            if (detalle == null)
                return ServiceResult<bool>.Fail(
                    ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(), "Línea de trabajo no encontrada.");

            detalle.Eliminado = true;
            await _detalleRepo.UpdateAsync(detalle);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        private static DetalleTrabajoDto MapToDto(DetalleTrabajo d) => new()
        {
            Id                 = d.Id,
            TrabajoId          = d.TrabajoId,
            ProductoId         = d.ProductoId,
            Concepto           = d.Concepto,
            Cantidad           = d.Cantidad,
            PrecioUnitario     = d.PrecioUnitario,
            DescuentoPorcentaje = d.DescuentoPorcentaje,
            ImpuestoPorcentaje = d.ImpuestoPorcentaje,
            EsManoObra         = d.EsManoObra,
            EstadoMaterial     = d.EstadoMaterial,
            TarifaHoraId       = d.TarifaHoraId,
            PrecioHoraAplicado = d.PrecioHoraAplicado,
            HorasAplicadas     = d.HorasAplicadas
        };
    }
}
