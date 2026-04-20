namespace Talleres360.Notifications.Api.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatario, string asunto, string mensajeHtml, string? nombreRemitente = null);
    }
}
