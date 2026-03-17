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
            To = "alumno.648000@ies-azarquiel.es", //Deberia ser destinatario pero Resend no permite dominios no verificados en pruebas
            Subject = asunto,
            HtmlBody = mensajeHtml
        };

        await _resend.EmailSendAsync(message);
    }

    public async Task EnviarEmailBienvenidaAsync(string email, string nombre, string linkVerificacion)
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "Templates", "EmailBienvenida.html");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("No se encontró la plantilla de email.");

        string template = await File.ReadAllTextAsync(filePath);

        string cuerpoHtml = template
            .Replace("{{Nombre}}", nombre)
            .Replace("{{Link}}", linkVerificacion);

        await EnviarEmailAsync(email, "¡Bienvenido a Talleres360!", cuerpoHtml);
    }
}