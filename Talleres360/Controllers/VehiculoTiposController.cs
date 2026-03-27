using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Responses;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Interfaces.Vehiculos;

namespace Talleres360.Controllers
{
    [Route("api/v1/vehiculos/tipos")]
    [ApiController]
    [Authorize]
    public class VehiculoTiposController : ControllerBase
    {
        private readonly IVehiculoTipoService _vehiculoTipoService;

        public VehiculoTiposController(IVehiculoTipoService vehiculoTipoService)
        {
            _vehiculoTipoService = vehiculoTipoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ServiceResult<List<VehiculoTipoDto>> resultado = await _vehiculoTipoService.ObtenerTiposVehiculoAsync();

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));
            }

            return Ok(ApiResponse<List<VehiculoTipoDto>>.Ok(resultado.Data!, "Tipos de vehículo recuperados correctamente."));
        }
    }
}
