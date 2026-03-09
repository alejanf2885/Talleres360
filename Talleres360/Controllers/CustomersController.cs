using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos.Clientes;
using Talleres360.Interfaces.Clientes;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Services.Seguridad;
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
        private readonly ISuscripcionGuardService _suscripcionGuard;

        public CustomersController(
            ICustomerService customerService,
            IUserContextService userContext,
            ISuscripcionGuardService suscripcionGuard)
        {
            _customerService = customerService;
            _userContext = userContext;
            _suscripcionGuard = suscripcionGuard;
        }

        // POST: api/v1/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearClienteRequest request)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            (bool PuedeAcceder, string Mensaje) guard = await _suscripcionGuard.ValidarAccesoEscrituraAsync(tallerId.Value);
            if (!guard.PuedeAcceder)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = guard.Mensaje });
            }

            (bool Success, string Message, Cliente? Cliente) resultado = await _customerService.CrearClienteAsync(tallerId.Value, request);

            if (!resultado.Success)
            {
                return BadRequest(new { mensaje = resultado.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = resultado.Cliente?.Id }, resultado.Cliente);
        }

        // GET: api/v1/customers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            IEnumerable<Cliente> clientes = await _customerService.ObtenerTodosAsync(tallerId.Value);
            return Ok(clientes);
        }

        // POST: api/v1/customers/search
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] BusquedaClienteRequest filtros)
        {
            int? tallerId = _userContext.GetTallerId();
            if (tallerId == null) return Unauthorized();

            IEnumerable<Cliente> clientes = await _customerService.ObtenerTodosAsync(tallerId.Value, filtros.Texto);
            return Ok(clientes);
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
    }
}