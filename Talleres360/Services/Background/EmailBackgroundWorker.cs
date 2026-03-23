using Talleres360.Dtos.Background;
using Talleres360.Interfaces.Background;
using Talleres360.Interfaces.Emails;

namespace Talleres360.Services.Background
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundWorker> _logger;

        public EmailBackgroundWorker(
            IBackgroundTaskQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailBackgroundWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    TareaBackground tarea = await _queue.DesencolarAsync(ct);

                    using IServiceScope scope = _scopeFactory.CreateScope();

                    if (tarea is EnviarEmailTask emailTask)
                    {
                        IEmailService emailService = scope.ServiceProvider
                            .GetRequiredService<IEmailService>();

                        await emailService.EnviarEmailAsync(
                            emailTask.Destinatario,
                            emailTask.Asunto,
                            emailTask.HtmlBody);

                        _logger.LogInformation(
                            "Email enviado a {Destinatario}",
                            emailTask.Destinatario);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando tarea en background");
                }
            }
        }
    }
}