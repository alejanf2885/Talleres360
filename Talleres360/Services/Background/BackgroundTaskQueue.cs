using System.Threading.Channels;
using Talleres360.Dtos.Background;
using Talleres360.Interfaces.Background;

namespace Talleres360.Services.Background
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<TareaBackground> _queue =
            Channel.CreateUnbounded<TareaBackground>();

        public void Encolar(TareaBackground tarea) =>
            _queue.Writer.TryWrite(tarea);

        public async Task<TareaBackground> DesencolarAsync(CancellationToken ct) =>
            await _queue.Reader.ReadAsync(ct);
    }
}