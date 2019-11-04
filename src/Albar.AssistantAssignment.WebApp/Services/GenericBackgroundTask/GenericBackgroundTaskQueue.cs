using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Services.GenericBackgroundTask
{
    public class GenericBackgroundTaskQueue : IGenericBackgroundTaskQueue
    {
        private readonly ILogger<GenericBackgroundTaskQueue> _logger;
        private readonly ConcurrentQueue<BackgroundTask> _queue = new ConcurrentQueue<BackgroundTask>();
        private readonly SemaphoreSlim _queueLock = new SemaphoreSlim(0);

        public GenericBackgroundTaskQueue(ILogger<GenericBackgroundTaskQueue> logger)
        {
            _logger = logger;
        }

        public void Enqueue(Func<IServiceProvider, CancellationToken, Task> task)
        {
            EnqueueBackgroundTask(new BackgroundTask(task));
        }

        public void EnqueueParallel(Func<IServiceProvider, CancellationToken, Task> task)
        {
            EnqueueBackgroundTask(new BackgroundTask(task)
            {
                RunInParallel = true
            });
        }
        
        public void EnqueueBackgroundTask(BackgroundTask task)
        {
            _queue.Enqueue(task);
            _logger.LogInformation("a background task queued");
            _queueLock.Release();
        }

        public async Task<BackgroundTask> DequeueAsync(CancellationToken token)
        {
            await _queueLock.WaitAsync(token);
            _queue.TryDequeue(out var task);
            _logger.LogInformation("a background task dequeued");
            return task;
        }
    }
}