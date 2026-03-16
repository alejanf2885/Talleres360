using Resend;
using System.Net.Mail;
using Talleres360.Interfaces.Emails;

public class ResendEmailService : IEmailService
{
    private readonly Resend.IResend _resend;
        private readonly IConfiguration _config;

    public ResendEmailService(Resend.IResend resend, IConfiguration config)
    {
        _resend = resend;
        _config = config;
    }

    public async Task EnviarEmailAsync(string destinatario, string asunto, string mensajeHtml, string? nombreRemitente = null)
    {
        var remitente = nombreRemitente ?? _config["ResendSettings:DefaultSenderName"] ?? "Talleres360";
        var emailTecnico = _config["ResendSettings:TechnicalEmail"] ?? "onboarding@resend.dev";

        var message = new EmailMessage
        {
            From = $"{remitente} <{emailTecnico}>",
            To = destinatario,
            Subject = asunto,
            HtmlBody = mensajeHtml
        };

        await _resend.EmailSendAsync(message);
    }
}