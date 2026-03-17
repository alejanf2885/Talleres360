using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres360.Dtos;
using Talleres360.Dtos.Talleres;
using Talleres360.Interfaces.Seguridad;
using Talleres360.Interfaces.Talleres;
using Talleres360.Models; 

namespace Talleres360.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopsController : ControllerBase
    {
        private readonly ITallerService _tallerService;
        private readonly ITallerRepository _tallerRepo;
        private readonly IUserContextService _userContext;

        public WorkshopsController(
            ITallerService tallerService,
            ITallerRepository tallerRepo,
            IUserContextService userContext)
        {
            _tallerService = tallerService;
            _tallerRepo = tallerRepo;
            _userContext = userContext;
        }

        // GET: api/v1/workshops/my-workshop
        [HttpGet("my-workshop")]
        public async Task<IActionResult> GetMyWorkshop()
        {
            int? tallerId = _userContext.GetTallerId();

            if (tallerId == null)
                return Unauthorized();

            Taller? taller = await _tallerRepo.GetByIdAsync(tallerId.Value);

            if (taller == null)
                return NotFound(new { Message = "Taller no encontrado." });

            // MAPEAMOS A DTO 
            WorkshopDto dto = new WorkshopDto
            {
                Id = taller.Id,
                Nombre = taller.Nombre,
                CIF = taller.CIF,
                Direccion = taller.Direccion,
                Localidad = taller.Localidad,
                Telefono = taller.Telefono,
                PerfilConfigurado = taller.PerfilConfigurado,
                TipoSuscripcion = taller.TipoSuscripcion,
                Logo = taller.Logo

            };

            return Ok(dto);
        }

        // PUT: api/v1/workshops/config
        [HttpPut("config")]
        public async Task<IActionResult> ConfigurarTaller([FromForm] ConfigurarTallerRequest request)
        {
            int? tallerId = _userContext.GetTallerId();

            if (tallerId == null)
                return Unauthorized(new { Error = "No se pudo identificar el taller desde el token." });

            bool success = await _tallerService.ConfigurarPerfilAsync(
                tallerId.Value,
                request.CIF,
                request.Direccion,
                request.Localidad,
                request.Telefono,
                request.Logo
            );

            if (!success)
                return BadRequest(new { Error = "Error al actualizar los datos. Verifica que el taller existe." });

            return Ok(new { Mensaje = "Perfil del taller configurado correctamente." });
        }
    }
}