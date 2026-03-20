using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;

namespace Talleres360.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiculosController : ControllerBase
    {
        private readonly IVehiculoService _vehiculoService;
        private readonly IUserContextService _userContext;

        public VehiculosController(IVehiculoService vehiculoService, IUserContextService userContext)
        {
            _vehiculoService = vehiculoService;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? matricula = null)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            VehiculoFiltroDto filtro = new VehiculoFiltroDto { Matricula = matricula };
            PagedResponse<VehiculoDetalle> response = await _vehiculoService.GetAllDetalleByTallerPagedAsync(tallerId.Value, page, size, filtro);

            return Ok(ApiResponse<PagedResponse<VehiculoDetalle>>.Ok(response, "Listado de vehículos recuperado correctamente."));
        }

        [TallerAuthorize<IVehiculoRepository>]
        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<VehiculoDetalle> resultado = await _vehiculoService.GetDetalleByIdAsync(tallerId.Value, id);

            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return Ok(ApiResponse<VehiculoDetalle>.Ok(resultado.Data!, "Datos del vehículo obtenidos."));
        }

        [RequiereSuscripcionActiva]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Vehiculo request)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<VehiculoDetalle> resultado = await _vehiculoService.RegistrarVehiculoAsync(tallerId.Value, request);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return CreatedAtAction(nameof(GetById),
                new { id = resultado.Data!.Id },
                ApiResponse<VehiculoDetalle>.Ok(resultado.Data!, "¡Vehículo registrado con éxito!"));
        }

        [TallerAuthorize<IVehiculoRepository>]
        [RequiereSuscripcionActiva]
        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update(int id, [FromBody] Vehiculo request)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<VehiculoDetalle> resultado = await _vehiculoService.ActualizarVehiculoAsync(tallerId.Value, id, request);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return Ok(ApiResponse<VehiculoDetalle>.Ok(resultado.Data!, "Los datos del vehículo han sido actualizados."));
        }
    }
}