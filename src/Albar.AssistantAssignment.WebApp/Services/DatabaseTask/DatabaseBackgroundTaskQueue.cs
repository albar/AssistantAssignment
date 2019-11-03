using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Albar.AssistantAssignment.WebApp.Services.DatabaseTask
{
    public class DatabaseBackgroundTaskQueue : IDatabaseBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<KeyValuePair<string, Func<AssignmentDatabase, CancellationToken, Task>>> _queue
            = new ConcurrentQueue<KeyValuePair<string, Func<AssignmentDatabase, CancellationToken, Task>>>();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public string Enqueue(Func<AssignmentDatabase, CancellationToken, Task> task)
        {
            var id = Guid.NewGuid().ToString();
            _queue.Enqueue(new KeyValuePair<string, Func<AssignmentDatabase, CancellationToken, Task>>(id, task));
            _signal.Release();
            return id;
        }

        public async Task<Func<AssignmentDatabase, CancellationToken, Task>> DequeueAsync(CancellationToken token)
        {
            await _signal.WaitAsync(token);
            _queue.TryDequeue(out var task);
            return async (database, cancellation) =>
            {
                var id = task.Key;
                // Notify
                await task.Value.Invoke(database, cancellation);
                // Notify
            };
        }
    }
}