using Talleres360.Dtos.Responses;
using Talleres360.Enums.Errors;
using Talleres360.Interfaces.Emails;
using Talleres360.Models;

namespace Talleres360.Services.Emails
{
    public class NotificacionService : INotificacionService
    {
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _config;

        public NotificacionService(IEmailService emailService, ITemplateService templateService, IConfiguration config)
        {
            _emailService = emailService;
            _templateService = templateService;
            _config = config;
        }

        public async Task<ServiceResult<bool>> EnviarBienvenidaAsync(Usuario usuario, string token)
        {
            try
            {
                string baseUrl = _config["FrontendSettings:Url"] ?? "https://localhost:4200";
                string link = $"{baseUrl}/auth/verify-email?token={token}";

                Dictionary<string, string> datos = new Dictionary<string, string>
            {
                { "{{Nombre}}", usuario.Nombre },
                { "{{Link}}", link }
            };

                string html = await _templateService.ObtenerPlantillaAsync("EmailBienvenida", datos);
                await _emailService.EnviarEmailAsync(usuario.Email, "¡Bienvenido a Talleres360!", html);

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(ErrorCode.SYS_ERROR_GENERICO.ToString(), "No se pudo enviar el email de bienvenida.");
            }
        }
    }
}