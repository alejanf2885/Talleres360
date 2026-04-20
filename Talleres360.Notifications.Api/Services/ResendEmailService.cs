using Resend;
using Talleres360.Notifications.Api.Interfaces;

namespace Talleres360.Notifications.Api.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _config;

        public ResendEmailService(IResend resend, IConfiguration config)
        {
            _resend = resend;
            _config = config;
        }

        public async Task EnviarEmailAsync(string destinatario, string asunto, string mensajeHtml, string? nombreRemitente = null)
        {
            var remitente = nombreRemitente ?? _config["ResendSettings:DefaultSenderName"] ?? "Talleres360";
            var emailTecnico = _config["ResendSettings:TechnicalEmail"] ?? "onboarding@resend.dev";

            EmailMessage message = new EmailMessage
            {
                From = $"{remitente} <{emailTecnico}>",
                To = "alumno.648000@ies-azarquiel.es", // Resend requiere dominio verificado para otros destinatarios
                
                Subject = asunto,
                HtmlBody = mensajeHtml
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
