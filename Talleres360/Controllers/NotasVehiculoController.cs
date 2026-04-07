using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos.NotasVehiculo;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.NotasVehiculo;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Controllers
{
    [Route("api/v1/notas-vehiculo")]
    [ApiController]
    [Authorize]
    public class NotasVehiculoController : ControllerBase
    {
        private readonly INotaVehiculoService _notaService;
        private readonly IUserContextService _userContextService;
        private readonly IVehiculoRepository _vehiculoRepository;

        public NotasVehiculoController(
            INotaVehiculoService notaService,
            IUserContextService userContextService,
            IVehiculoRepository vehiculoRepository)
        {
            _notaService = notaService;
            _userContextService = userContextService;
            _vehiculoRepository = vehiculoRepository;
        }

        [HttpGet("~/api/v1/vehiculos/{vehiculoId:int:min(1)}/notas")]
        public async Task<IActionResult> GetByVehiculo(int vehiculoId)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(vehiculoId, tallerId.Value);
            if (!vehiculoPertenece)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: ErrorCode.VEH_NO_ENCONTRADO.ToString(),
                    mensaje: "Vehículo no encontrado."));
            }

            ServiceResult<List<NotaVehiculoDto>> resultado = await _notaService.ObtenerPorVehiculoAsync(tallerId.Value, vehiculoId);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudieron recuperar las notas del vehículo."));
            }

            return Ok(ApiResponse<List<NotaVehiculoDto>>.Ok(resultado.Data ?? new List<NotaVehiculoDto>(), "Notas recuperadas correctamente."));
        }

        [HttpPost("~/api/v1/vehiculos/{vehiculoId:int:min(1)}/notas")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create(int vehiculoId, [FromBody] CrearNotaVehiculoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            int? usuarioId = _userContextService.GetUsuarioId();

            ServiceResult<NotaVehiculoDto> resultado = await _notaService.CrearAsync(tallerId.Value, vehiculoId, usuarioId, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo crear la nota."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<NotaVehiculoDto>.Ok(resultado.Data, "Nota creada correctamente."));
        }

        [TallerAuthorize<INotaVehiculoRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<NotaVehiculoDto> resultado = await _notaService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Nota no encontrada."));
            }

            return Ok(ApiResponse<NotaVehiculoDto>.Ok(resultado.Data!, "Nota recuperada correctamente."));
        }

        [TallerAuthorize<INotaVehiculoRepository>]
        [HttpPut("{id:int:min(1)}")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarNotaVehiculoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<NotaVehiculoDto> resultado = await _notaService.ActualizarAsync(tallerId.Value, id, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo actualizar la nota."));
            }

            return Ok(ApiResponse<NotaVehiculoDto>.Ok(resultado.Data!, "Nota actualizada correctamente."));
        }

        [TallerAuthorize<INotaVehiculoRepository>]
        [HttpPatch("{id:int:min(1)}/resolver")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Resolver(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<NotaVehiculoDto> resultado = await _notaService.ResolverAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo resolver la nota."));
            }

            return Ok(ApiResponse<NotaVehiculoDto>.Ok(resultado.Data!, "Nota resuelta correctamente."));
        }

        [TallerAuthorize<INotaVehiculoRepository>]
        [HttpDelete("{id:int:min(1)}")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _notaService.EliminarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo eliminar la nota."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Nota eliminada correctamente."));
        }
    }
}
