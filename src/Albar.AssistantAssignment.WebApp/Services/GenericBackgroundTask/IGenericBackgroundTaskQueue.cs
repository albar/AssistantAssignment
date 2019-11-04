using System;
using System.Threading;
using System.Threading.Tasks;

namespace Albar.AssistantAssignment.WebApp.Services.GenericBackgroundTask
{
    public interface IGenericBackgroundTaskQueue
    {
        void EnqueueBackgroundTask(BackgroundTask task);
        void Enqueue(Func<IServiceProvider, CancellationToken, Task> task);
        void EnqueueParallel(Func<IServiceProvider, CancellationToken, Task> task);
        Task<BackgroundTask> DequeueAsync(CancellationToken token);
    }
}