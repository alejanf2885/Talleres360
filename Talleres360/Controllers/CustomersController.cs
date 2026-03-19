using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters;
using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IUserContextService _userContext;

        public CustomersController(
            ICustomerService customerService,
            IUserContextService userContext)
        {
            _customerService = customerService;
            _userContext = userContext;
        }

        [HttpPost]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create([FromBody] CrearClienteRequest request)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<Cliente> resultado = await _customerService.CrearClienteAsync(tallerId.Value, request);

            if (!resultado.Success)
            {
                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al crear el cliente."
                ));
            }

            return CreatedAtAction(nameof(GetById), new { id = resultado.Data?.Id }, resultado.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? buscar = null)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            PaginationParams pagination = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            PagedResponse<Cliente> resultado = await _customerService.ObtenerTodosPagedAsync(tallerId.Value, pagination, buscar);

            return Ok(resultado);
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] BusquedaClienteRequest filtros, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            PaginationParams pagination = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            PagedResponse<Cliente> resultado = await _customerService.ObtenerTodosPagedAsync(tallerId.Value, pagination, filtros.Texto);

            return Ok(resultado);
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            Cliente? cliente = await _customerService.ObtenerPorIdAsync(tallerId.Value, id);

            if (cliente == null)
            {
                return NotFound(new ApiErrorResponse(
                    codigo: ErrorCode.CUST_NO_ENCONTRADO.ToString(),
                    mensaje: "Cliente no encontrado o no pertenece a su taller."
                ));
            }

            return Ok(cliente);
        }

        [HttpPut("{id:int:min(1)}")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarClienteRequest request)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<Cliente> resultado = await _customerService.ActualizarClienteAsync(tallerId.Value, id, request);

            if (!resultado.Success)
            {
                if (resultado.ErrorCode == ErrorCode.CUST_NO_ENCONTRADO.ToString())
                {
                    return NotFound(new ApiErrorResponse(resultado.ErrorCode, resultado.Message!));
                }

                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al actualizar el cliente."
                ));
            }

            return Ok(new { mensaje = resultado.Message, cliente = resultado.Data });
        }

        [HttpDelete("{id:int:min(1)}")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ServiceResult<bool> resultado = await _customerService.EliminarClienteAsync(tallerId.Value, id);

            if (!resultado.Success)
            {
                if (resultado.ErrorCode == ErrorCode.CUST_NO_ENCONTRADO.ToString())
                {
                    return NotFound(new ApiErrorResponse(resultado.ErrorCode, resultado.Message!));
                }

                return BadRequest(new ApiErrorResponse(
                    codigo: resultado.ErrorCode ?? ErrorCode.SYS_ERROR_GENERICO.ToString(),
                    mensaje: resultado.Message ?? "Error al eliminar el cliente."
                ));
            }

            return Ok(new { mensaje = resultado.Message });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            int? tallerId = _userContext.GetTallerId();
            if (!tallerId.HasValue) return Unauthorized();

            ClienteStatsResponse stats = await _customerService.ObtenerEstadisticasAsync(tallerId.Value);

            return Ok(stats);
        }
    }
}