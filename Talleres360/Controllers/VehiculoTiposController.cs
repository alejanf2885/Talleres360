using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.API.Controllers
{
    [Route("api/v1/vehiculos/tipos")]
    [ApiController]
    [Authorize]
    public class VehiculoTiposController : ControllerBase
    {
        private readonly IVehiculoMaestroService _vehiculoMaestroService;

        public VehiculoTiposController(IVehiculoMaestroService vehiculoMaestroService)
        {
            _vehiculoMaestroService = vehiculoMaestroService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ServiceResult<List<VehiculoTipoDto>> resultado = await _vehiculoMaestroService.ObtenerTiposVehiculoAsync();

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return Ok(ApiResponse<List<VehiculoTipoDto>>.Ok(resultado.Data!, "Tipos de vehículo recuperados correctamente."));
        }
    }
}
