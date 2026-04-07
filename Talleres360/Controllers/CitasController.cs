using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Citas;
using Talleres360.Dtos.Responses;
using Talleres360.Enums;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Citas;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class CitasController : ControllerBase
    {
        private readonly ICitaService _citaService;
        private readonly IUserContextService _userContextService;
        private readonly IVehiculoRepository _vehiculoRepository;

        public CitasController(
            ICitaService citaService,
            IUserContextService userContextService,
            IVehiculoRepository vehiculoRepository)
        {
            _citaService        = citaService;
            _userContextService = userContextService;
            _vehiculoRepository = vehiculoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paginacion,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] CitaEstado? estado = null,
            [FromQuery] int? vehiculoId = null)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            int? vehiculoIdFiltrado = vehiculoId;

            if (vehiculoIdFiltrado.HasValue)
            {
                bool vehiculoPertenece = await _vehiculoRepository.PerteneceATallerAsync(vehiculoIdFiltrado.Value, tallerId.Value);
                if (!vehiculoPertenece)
                {
                    vehiculoIdFiltrado = null;
                }
            }

            PagedResponse<CitaDto> citas = await _citaService.ObtenerTodasAsync(
                tallerId.Value,
                paginacion,
                fechaDesde,
                fechaHasta,
                estado,
                vehiculoIdFiltrado);

            return Ok(ApiResponse<PagedResponse<CitaDto>>.Ok(citas, "Listado de citas recuperado correctamente."));
        }

        [TallerAuthorize<ICitaRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<CitaDto> resultado = await _citaService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.CITA_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Cita no encontrada."));
            }

            return Ok(ApiResponse<CitaDto>.Ok(resultado.Data!, "Cita recuperada correctamente."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearCitaRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<CitaDto> resultado = await _citaService.CrearAsync(tallerId.Value, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al crear cita."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<CitaDto>.Ok(resultado.Data, "Cita creada correctamente."));
        }

        [TallerAuthorize<ICitaRepository>]
        [RequiereSuscripcionActiva]
        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarCitaRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<CitaDto> resultado = await _citaService.ActualizarAsync(tallerId.Value, id, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar cita."));
            }

            return Ok(ApiResponse<CitaDto>.Ok(resultado.Data!, "Cita actualizada correctamente."));
        }

        [TallerAuthorize<ICitaRepository>]
        [RequiereSuscripcionActiva]
        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _citaService.EliminarAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar cita."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Cita eliminada correctamente."));
        }

        [TallerAuthorize<ICitaRepository>]
        [RequiereSuscripcionActiva]
        [HttpPost("{id:int:min(1)}/convertir-a-trabajo")]
        public async Task<IActionResult> ConvertirATrabajo(int id, [FromBody] ConvertirCitaTrabajoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            int? usuarioId = _userContextService.GetUsuarioId();

            ServiceResult<CitaTrabajoDto> resultado = await _citaService.ConvertirATrabajoAsync(tallerId.Value, id, usuarioId, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo convertir la cita en trabajo."));
            }

            return Ok(ApiResponse<CitaTrabajoDto>.Ok(resultado.Data!, "Cita convertida a trabajo correctamente."));
        }
    }
}
