using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Services
{
    public class QueuedParallelGeneticAlgorithmBackgroundTask : BackgroundService
    {
        public QueuedParallelGeneticAlgorithmBackgroundTask(
            IGeneticAlgorithmBackgroundTaskQueue queue,
            ILogger<QueuedParallelGeneticAlgorithmBackgroundTask> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        private readonly IGeneticAlgorithmBackgroundTaskQueue _queue;
        private readonly ILogger<QueuedParallelGeneticAlgorithmBackgroundTask> _logger;

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var runningTasks = new List<GeneticAlgorithmTask>();
            while (!token.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting Task");
                var task = await _queue.DequeueAsync(token);
                _logger.LogInformation("Dequeuing Task");
                try
                {
                    var taskId = Guid.NewGuid().ToString();
                    var tokenSource = new CancellationTokenSource();

                    _logger.LogInformation("Running Task");
                    var runningTask = new GeneticAlgorithmTask
                    {
                        TaskId = taskId,
                        TokenSource = tokenSource,
                        RunningTask = task.Invoke(taskId, tokenSource).ContinueWith(backgroundTask =>
                        {
                            _queue.BackgroundTaskFinished(taskId, backgroundTask.Result);
                            DisposeAndRemoveTask(taskId);
                        })
                    };
                    _logger.LogInformation("Task is Running");
                    runningTasks.Add(runningTask);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    throw;
                }
            }

            var tasks = runningTasks.Select(task =>
            {
                task.TokenSource.Cancel();
                return task.RunningTask;
            }).ToList();

            await Task.WhenAll(tasks);

            void DisposeAndRemoveTask(string taskId)
            {
                var runningTask = runningTasks.First(task => task.TaskId == taskId);
                if (runningTask == null) return;
                runningTask.TokenSource?.Dispose();
                runningTasks.Remove(runningTask);
            }
        }

        private class GeneticAlgorithmTask
        {
            public string TaskId { get; set; }
            public CancellationTokenSource TokenSource { get; set; }
            public Task RunningTask { get; set; }
        }
    }
}