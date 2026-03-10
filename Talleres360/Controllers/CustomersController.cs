using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.API.Filters; // <-- Necesario para el nuevo filtro
using Talleres360.Dtos;
using Talleres360.Dtos.Clientes;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Models;

namespace Talleres360.API.Controllers
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

        // POST: api/v1/customers
        [HttpPost]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Create([FromBody] CrearClienteRequest request)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            // Directos a la lógica de negocio
            (bool Success, string Message, Cliente? Cliente) resultado = await _customerService.CrearClienteAsync(tallerId.Value, request);

            if (!resultado.Success)
            {
                return BadRequest(new { mensaje = resultado.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = resultado.Cliente?.Id }, resultado.Cliente);
        }

        // GET: api/v1/customers
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? buscar = null)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            var pagination = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var resultado = await _customerService.ObtenerTodosPagedAsync(tallerId.Value, pagination, buscar);
            return Ok(resultado);
        }

        // POST: api/v1/customers/search
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] BusquedaClienteRequest filtros, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            var pagination = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var resultado = await _customerService.ObtenerTodosPagedAsync(tallerId.Value, pagination, filtros.Texto);
            return Ok(resultado);
        }

        // GET: api/v1/customers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            Cliente? cliente = await _customerService.ObtenerPorIdAsync(tallerId.Value, id);

            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado o no pertenece a su taller." });

            return Ok(cliente);
        }



        // PUT: api/v1/customers/5
        [HttpPut("{id}")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarClienteRequest request)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            var resultado = await _customerService.ActualizarClienteAsync(tallerId.Value, id, request);

            if (!resultado.Success)
                return BadRequest(new { mensaje = resultado.Message });

            return Ok(new { mensaje = resultado.Message, cliente = resultado.Cliente });
        }

        // DELETE: api/v1/customers/5
        [HttpDelete("{id}")]
        [RequiereSuscripcionActiva]
        public async Task<IActionResult> Delete(int id)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            var resultado = await _customerService.EliminarClienteAsync(tallerId.Value, id);

            if (!resultado.Success)
                return BadRequest(new { mensaje = resultado.Message });

            return Ok(new { mensaje = resultado.Message });
        }

        // GET: api/v1/customers/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            ClienteStatsResponse stats = await _customerService.ObtenerEstadisticasAsync(tallerId.Value);
            return Ok(stats);
        }
    }
}