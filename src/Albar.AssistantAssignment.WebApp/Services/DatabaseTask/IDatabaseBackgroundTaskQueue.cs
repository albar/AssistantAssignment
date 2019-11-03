using System;
using System.Threading;
using System.Threading.Tasks;

namespace Albar.AssistantAssignment.WebApp.Services.DatabaseTask
{
    public interface IDatabaseBackgroundTaskQueue
    {
        string Enqueue(Func<AssignmentDatabase, CancellationToken, Task> task);
        Task<Func<AssignmentDatabase, CancellationToken, Task>> DequeueAsync(CancellationToken token);
    }
}