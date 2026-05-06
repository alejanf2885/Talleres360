using Microsoft.AspNetCore.Mvc;
using Talleres360.Notifications.Api.Dtos;
using Talleres360.Notifications.Api.Interfaces;

namespace Talleres360.Notifications.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;

        public NotificacionesController(IEmailService emailService, ITemplateService templateService)
        {
            _emailService = emailService;
            _templateService = templateService;
        }

        [HttpPost("bienvenida")]
        public async Task<IActionResult> EnviarBienvenida([FromBody] EnviarBienvenidaRequest request)
        {
            var datos = new Dictionary<string, string>
            {
                { "{{Nombre}}", request.Nombre },
                { "{{Link}}", request.Link }
            };

            string html = await _templateService.ObtenerPlantillaAsync("EmailBienvenida", datos);
            await _emailService.EnviarEmailAsync(request.Email, "¡Bienvenido a Talleres360!", html);

            return Ok(new { mensaje = "Email de bienvenida enviado." });
        }
    }
}
