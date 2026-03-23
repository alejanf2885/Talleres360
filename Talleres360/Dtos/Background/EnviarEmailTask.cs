namespace Talleres360.Dtos.Background
{
    public class EnviarEmailTask : TareaBackground
    {
        public string Destinatario { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
    }
}