using Talleres360.Dtos.Background;

namespace Talleres360.Interfaces.Background
{
    public interface IBackgroundTaskQueue
    {
        void Encolar(TareaBackground tarea);
        Task<TareaBackground> DesencolarAsync(CancellationToken ct);
    }
}