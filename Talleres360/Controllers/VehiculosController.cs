using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Filters;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;
using Talleres360.Extensions;

namespace Talleres360.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehiculosController : ControllerBase
    {
        private readonly IVehiculoService _vehiculoService;

        public VehiculosController(IVehiculoService vehiculoService)
        {
            _vehiculoService = vehiculoService;
        }

        [TallerAuthorize<IVehiculoRepository>]
        [HttpGet]
        public async Task<ActionResult<PagedResponse<VehiculoDetalle>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] VehiculoFiltroDto? filtro = null)
        {
            int? tallerId = User.GetTallerId();

            if (!tallerId.HasValue)
            {
                return Unauthorized("No se pudo identificar el taller del usuario.");
            }

            var response = await _vehiculoService.GetAllDetalleByTallerAsync(
                tallerId.Value,
                pageNumber,
                pageSize,
                filtro);

            return Ok(response);
        }
        [TallerAuthorize<IVehiculoRepository>]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VehiculoDetalle?>> GetById(int id)
        {
            var detalle = await _vehiculoService.GetDetalleByIdAsync(id);
            if (detalle == null) return NotFound();

            return Ok(detalle);
        }

        [TallerAuthorize<IVehiculoRepository>]
        [HttpGet("matricula/{matricula}")]
        public async Task<ActionResult<VehiculoDetalle?>> GetByMatricula(string matricula)
        {
            if (string.IsNullOrWhiteSpace(matricula)) return BadRequest("La matrícula es requerida.");

            int? tallerId = User.GetTallerId();
            var detalle = await _vehiculoService.GetDetalleByMatriculaAsync(matricula);

            if (detalle == null || detalle.TallerId != tallerId)
            {
                return NotFound();
            }

            return Ok(detalle);
        }

        [TallerAuthorize<IVehiculoRepository>]
        [HttpPost]
        public async Task<ActionResult<VehiculoDetalle>> Add([FromBody] Vehiculo? vehiculo)
        {
            if (vehiculo == null) return BadRequest("El cuerpo de la petición no puede estar vacío.");

            int? tId = User.GetTallerId();
            if (!tId.HasValue)
            {
                return Unauthorized();
            }

            vehiculo.TallerId = tId.Value;

            if (await _vehiculoService.ExistsAsync(vehiculo.Matricula))
            {
                return Conflict($"Ya existe un vehículo con matrícula {vehiculo.Matricula}");
            }

            await _vehiculoService.AddAsync(vehiculo);

            VehiculoDetalle? vehiculoActualizado = await _vehiculoService.GetDetalleByIdAsync(vehiculo.Id);

            if (vehiculoActualizado == null)
            {
                return StatusCode(500, "El vehículo se guardó correctamente, pero hubo un error al recuperar sus detalles.");
            }
            return CreatedAtAction(nameof(GetById), new { id = vehiculo.Id }, vehiculoActualizado);
        }

        [TallerAuthorize<IVehiculoRepository>]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<VehiculoDetalle>> Update(int id, [FromBody] Vehiculo? vehiculo)
        {
            if (vehiculo == null) return BadRequest("Los datos son requeridos.");

            if (id != vehiculo.Id) return BadRequest("El Id de la ruta no coincide con el del objeto.");

            int? tallerId = User.GetTallerId();
            if (tallerId == null)
                return Unauthorized();

            vehiculo.TallerId = tallerId.Value;

            await _vehiculoService.UpdateAsync(vehiculo);

            VehiculoDetalle? vehiculoActualizado = await _vehiculoService.GetDetalleByIdAsync(vehiculo.Id);

            if (vehiculoActualizado == null)
            {
                return StatusCode(500, "El vehículo se guardó correctamente, pero hubo un error al recuperar sus detalles.");
            }
            return Ok(vehiculoActualizado);
        }
    }
}