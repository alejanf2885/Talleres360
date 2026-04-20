using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Trabajos;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Trabajos;
using Talleres360.Interfaces.Data;

namespace Talleres360.Services.Trabajos
{
    public class TarifaHoraService : ITarifaHoraService
    {
        private readonly ITarifaHoraRepository _tarifaRepo;
        private readonly IUnitOfWork _unitOfWork;

        public TarifaHoraService(ITarifaHoraRepository tarifaRepo, IUnitOfWork unitOfWork)
        {
            _tarifaRepo = tarifaRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<TarifaHoraDto>> CrearAsync(int tallerId, int usuarioId, CrearTarifaHoraRequest request)
        {
            if (request.PrecioHora < 0)
            {
                return ServiceResult<TarifaHoraDto>.Fail(
                    ErrorCode.SYS_DATOS_INVALIDOS.ToString(),
                    "El precio por hora no puede ser negativo.");
            }

            var tarifa = new TarifaHora
            {
                TallerId      = tallerId,
                PrecioHora    = request.PrecioHora,
                Descripcion   = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
                FechaVigencia = request.FechaVigencia ?? DateOnly.FromDateTime(DateTime.UtcNow),
                Activa        = true,
                CreadoPorId   = usuarioId,
                FechaCreacion = DateTime.UtcNow
            };

            await _tarifaRepo.AddAsync(tarifa);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<TarifaHoraDto>.Ok(MapToDto(tarifa));
        }

        public async Task<ServiceResult<IEnumerable<TarifaHoraDto>>> ObtenerHistorialAsync(int tallerId)
        {
            var historial = await _tarifaRepo.ObtenerHistorialAsync(tallerId);
            return ServiceResult<IEnumerable<TarifaHoraDto>>.Ok(historial);
        }

        public async Task<ServiceResult<TarifaHoraDto?>> ObtenerActivaAsync(int tallerId)
        {
            TarifaHora? activa = await _tarifaRepo.ObtenerActivaAsync(tallerId);
            return ServiceResult<TarifaHoraDto?>.Ok(activa == null ? null : MapToDto(activa));
        }

        private static TarifaHoraDto MapToDto(TarifaHora t) => new()
        {
            Id            = t.Id,
            TallerId      = t.TallerId,
            PrecioHora    = t.PrecioHora,
            Descripcion   = t.Descripcion,
            FechaVigencia = t.FechaVigencia,
            Activa        = t.Activa,
            CreadoPorId   = t.CreadoPorId,
            FechaCreacion = t.FechaCreacion
        };
    }
}
