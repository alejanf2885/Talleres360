using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Vehiculos;
using Talleres360.Filters;
using Talleres360.Interfaces.Vehiculos;
using Talleres360.Models;
using Talleres360.Extensions; // Asegúrate de importar tus extensiones

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

        [HttpGet]
        public async Task<ActionResult<PagedResponse<VehiculoDetalle>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] VehiculoFiltroDto? filtro = null)
        {
            // Usamos la extensión para obtener el TallerId del Token de forma segura
            int tallerId = User.GetTallerId();

            var response = await _vehiculoService.GetAllDetalleByTallerAsync(
                tallerId,
                pageNumber,
                pageSize,
                filtro);

            return Ok(response);
        }

        [TallerAuthorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VehiculoDetalle?>> GetById(int id)
        {
            var detalle = await _vehiculoService.GetDetalleByIdAsync(id);
            if (detalle == null) return NotFound();

            return Ok(detalle);
        }

        [HttpGet("matricula/{matricula}")]
        public async Task<ActionResult<VehiculoDetalle?>> GetByMatricula(string matricula)
        {
            if (string.IsNullOrWhiteSpace(matricula)) return BadRequest("La matrícula es requerida.");

            int tallerId = User.GetTallerId();
            var detalle = await _vehiculoService.GetDetalleByMatriculaAsync(matricula);

            if (detalle == null || detalle.TallerId != tallerId)
            {
                return NotFound();
            }

            return Ok(detalle);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] Vehiculo? vehiculo)
        {
            // 1. Blindaje contra NULL
            if (vehiculo == null) return BadRequest("El cuerpo de la petición no puede estar vacío.");

            // 2. Seguridad: Forzamos el TallerId del Token (ignora lo que venga en el JSON)
            vehiculo.TallerId = User.GetTallerId();

            // 3. Validación de duplicados
            if (await _vehiculoService.ExistsAsync(vehiculo.Matricula))
            {
                return Conflict($"Ya existe un vehículo con matrícula {vehiculo.Matricula}");
            }

            await _vehiculoService.AddAsync(vehiculo);
            return CreatedAtAction(nameof(GetById), new { id = vehiculo.Id }, vehiculo);
        }

        [TallerAuthorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] Vehiculo? vehiculo)
        {
            if (vehiculo == null) return BadRequest("Los datos son requeridos.");

            if (id != vehiculo.Id) return BadRequest("El Id de la ruta no coincide con el del objeto.");

            vehiculo.TallerId = User.GetTallerId();

            await _vehiculoService.UpdateAsync(vehiculo);
            return NoContent();
        }
    }
}