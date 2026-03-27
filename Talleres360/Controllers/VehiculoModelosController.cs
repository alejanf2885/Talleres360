using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos.Modelo;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Controllers
{
    [Route("api/v1/vehiculos/modelos")]
    [ApiController]
    [Authorize]
    public class VehiculoModelosController : ControllerBase
    {
        private readonly IModeloService _modeloService;
        private readonly IUserContextService _userContextService;

        public VehiculoModelosController(
            IModeloService modeloService,
            IUserContextService userContextService)
        {
            _modeloService = modeloService;
            _userContextService = userContextService;
        }

        [HttpGet("{marcaId:int:min(1)}")]
        public async Task<IActionResult> GetByMarca(int marcaId)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<List<ModeloVehiculoDto>> resultado = await _modeloService.ObtenerModelosPorMarcaAsync(tallerId.Value, marcaId);

            if (!resultado.Success)
            {
                return NotFound(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return Ok(ApiResponse<List<ModeloVehiculoDto>>.Ok(resultado.Data!, "Modelos recuperados correctamente."));
        }

        
        [HttpPost]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create([FromBody] CrearModeloVehiculoDto crearModeloDto)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            bool esOficial = false;
            string? rol = _userContextService.GetRol();

            if (rol != null && rol.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase))
            {
                esOficial = true;
            }

            ServiceResult<ModeloVehiculoDto> resultado = await _modeloService.CrearModeloAsync(tallerId.Value, crearModeloDto, esOficial);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return Ok(ApiResponse<ModeloVehiculoDto>.Ok(resultado.Data, "Modelo creado correctamente."));
        }

        [HttpPut("{id:int:min(1)}")]
        [TallerAuthorize<IModeloRepository>]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarModeloVehiculoDto actualizarModeloDto)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<ModeloVehiculoDto> resultado = await _modeloService.ActualizarModeloAsync(tallerId.Value, id, actualizarModeloDto);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar el modelo."));
            }

            return Ok(ApiResponse<ModeloVehiculoDto>.Ok(resultado.Data!, "Modelo actualizado correctamente."));
        }

        [HttpDelete("{id:int:min(1)}")]
        [TallerAuthorize<IModeloRepository>]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<bool> resultado = await _modeloService.EliminarModeloAsync(tallerId.Value, id);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar el modelo."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "¡Modelo eliminado con éxito!"));
        }
    }
}
