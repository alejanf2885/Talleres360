using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.API.Controllers
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
    }
}
