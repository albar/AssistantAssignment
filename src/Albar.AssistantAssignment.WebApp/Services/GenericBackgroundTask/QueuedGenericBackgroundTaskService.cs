using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Services.GenericBackgroundTask
{
    public class QueuedGenericBackgroundTaskService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IGenericBackgroundTaskQueue _queue;
        private readonly ILogger<QueuedDatabaseBackgroundTask> _logger;

        public QueuedGenericBackgroundTaskService(IServiceProvider provider,
            IGenericBackgroundTaskQueue queue,
            ILogger<QueuedDatabaseBackgroundTask> logger)
        {
            _provider = provider;
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var parallelTasks = new List<Task>();
            while (!stoppingToken.IsCancellationRequested)
            {
                var backgroundTask = await _queue.DequeueAsync(stoppingToken);
                var task = backgroundTask.Invoke(_provider, stoppingToken);
                if (backgroundTask.RunInParallel)
                {
                    parallelTasks.Add(task);
                }
                else
                {
                    try
                    {
                        await task;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);
                    }
                }
            }

            try
            {
                await Task.WhenAll(parallelTasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}