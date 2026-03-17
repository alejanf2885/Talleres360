namespace Talleres360.Interfaces.Emails
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatario, string asunto, string mensajeHtml, string? nombreRemitente = null);

        Task EnviarEmailBienvenidaAsync(string email, string nombre, string linkVerificacion);
    }
}
