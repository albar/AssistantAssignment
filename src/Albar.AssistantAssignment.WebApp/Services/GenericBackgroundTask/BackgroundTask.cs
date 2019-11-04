using System;
using System.Threading;
using System.Threading.Tasks;

namespace Albar.AssistantAssignment.WebApp.Services.GenericBackgroundTask
{
    public class BackgroundTask
    {
        private readonly Func<IServiceProvider, CancellationToken, Task> _task;

        public BackgroundTask(Func<IServiceProvider, CancellationToken, Task> task)
        {
            _task = task;
        }
        public async Task Invoke(IServiceProvider provider, CancellationToken token)
        {
            await _task.Invoke(provider, token);
        }

        public bool RunInParallel { get; set; } = false;
    }
}