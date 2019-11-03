using System;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.WebApp.Services.DatabaseTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Services
{
    public class QueuedDatabaseBackgroundTask : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IDatabaseBackgroundTaskQueue _queue;
        private readonly ILogger<QueuedDatabaseBackgroundTask> _logger;

        public QueuedDatabaseBackgroundTask(
            IServiceProvider services,
            IDatabaseBackgroundTaskQueue queue,
            ILogger<QueuedDatabaseBackgroundTask> logger)
        {
            _services = services;
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _logger.LogInformation("Acquiring Task");
                var task = await _queue.DequeueAsync(token);
                _logger.LogInformation("Task Acquired");
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var database = scope.ServiceProvider.GetRequiredService<AssignmentDatabase>();
                        
                        _logger.LogInformation("Running Task");
                        await task.Invoke(database, token);
                        _logger.LogInformation("Task Completed");
                    }
                }
                catch(Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }
    }
}