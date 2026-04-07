using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Presupuestos;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Presupuestos;
using Talleres360.Interfaces.Seguridad;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PresupuestosController : ControllerBase
    {
        private readonly IPresupuestoService _presupuestoService;
        private readonly IUserContextService _userContextService;

        public PresupuestosController(IPresupuestoService presupuestoService, IUserContextService userContextService)
        {
            _presupuestoService = presupuestoService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginacion)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            PagedResponse<PresupuestoDto> presupuestos = await _presupuestoService.ObtenerTodosAsync(tallerId.Value, paginacion);
            return Ok(ApiResponse<PagedResponse<PresupuestoDto>>.Ok(presupuestos, "Listado de presupuestos recuperado correctamente."));
        }

        [TallerAuthorize<IPresupuestoRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<PresupuestoDto> resultado = await _presupuestoService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ENTIDAD_NO_ENCONTRADA.ToString(),
                    mensaje: resultado.Message ?? "Presupuesto no encontrado."));
            }

            return Ok(ApiResponse<PresupuestoDto>.Ok(resultado.Data!, "Presupuesto recuperado correctamente."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearPresupuestoRequest request)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<PresupuestoDto> resultado = await _presupuestoService.CrearAsync(tallerId.Value, request);
            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "No se pudo crear el presupuesto."));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<PresupuestoDto>.Ok(resultado.Data, "Presupuesto creado correctamente."));
        }
    }
}
