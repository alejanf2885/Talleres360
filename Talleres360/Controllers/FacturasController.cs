using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Facturacion;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Filters;
using Talleres360.Interfaces.Facturacion;
using Talleres360.Interfaces.Seguridad;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class FacturasController : ControllerBase
    {
        private readonly IFacturacionService _facturacionService;
        private readonly IUserContextService _userContext;

        public FacturasController(IFacturacionService facturacionService, IUserContextService userContext)
        {
            _facturacionService = facturacionService;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<PagedResponse<FacturaDto>> resultado = await _facturacionService.ObtenerTodosAsync(tallerId.Value, pagination);
            return Ok(ApiResponse<PagedResponse<FacturaDto>>.Ok(resultado.Data!, "Listado de facturas recuperado."));
        }

        [HttpGet("{id:int:min(1)}")]
        [TallerAuthorize<IFacturaRepository>]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<FacturaDto> resultado = await _facturacionService.ObtenerPorIdAsync(tallerId.Value, id);
            if (!resultado.Success)
                return NotFound(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));

            return Ok(ApiResponse<FacturaDto>.Ok(resultado.Data!, "Factura recuperada."));
        }

        [HttpGet("trabajo/{trabajoId:int:min(1)}")]
        public async Task<IActionResult> GetByTrabajo(int trabajoId)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<FacturaDto> resultado = await _facturacionService.ObtenerPorTrabajoAsync(tallerId.Value, trabajoId);
            if (!resultado.Success)
                return NotFound(new ApiErrorResponse(resultado.ErrorCode!, resultado.Message!));

            return Ok(ApiResponse<FacturaDto>.Ok(resultado.Data!, "Factura recuperada."));
        }
    }
}
