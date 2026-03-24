using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.API.Controllers
{
    [Route("api/v1/vehiculos/marcas")]
    [ApiController]
    [Authorize]
    public class VehiculoMarcasController : ControllerBase
    {
        private readonly IMarcaService _marcaService;
        private readonly IUserContextService _userContextService;

        public VehiculoMarcasController(
            IMarcaService marcaService,
            IUserContextService userContextService)
        {
            _marcaService = marcaService;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int? tallerId = _userContextService.GetTallerId();
            if (!tallerId.HasValue)
            {
                return Unauthorized();
            }

            ServiceResult<List<MarcaVehiculoDto>> resultado = await _marcaService.ObtenerMarcasAsync(tallerId.Value);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return Ok(ApiResponse<List<MarcaVehiculoDto>>.Ok(resultado.Data!, "Marcas recuperadas correctamente."));
        }
    }
}
