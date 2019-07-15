using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Hubs;
using Albar.AssistantAssignment.WebApp.Models;
using Bunnypro.GeneticAlgorithm.Primitives;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public class GeneticAlgorithmBackgroundTaskQueue : IGeneticAlgorithmBackgroundTaskQueue
    {
        private readonly IServiceProvider _services;
        private readonly IHubContext<GeneticAlgorithmNotificationHub, IGeneticAlgorithmTaskListener> _notification;
        private readonly ILogger<GeneticAlgorithmBackgroundTaskQueue> _logger;

        private readonly HashSet<AssignmentDataRepository> _repositories =
            new HashSet<AssignmentDataRepository>();

        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _tasksQueue =
            new ConcurrentQueue<Func<CancellationToken, Task>>();

        private readonly Dictionary<string, IGeneticAlgorithmTask> _geneticAlgorithmTasks =
            new Dictionary<string, IGeneticAlgorithmTask>();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public GeneticAlgorithmBackgroundTaskQueue(
            IServiceProvider services,
            IHubContext<GeneticAlgorithmNotificationHub, IGeneticAlgorithmTaskListener> notification,
            ILogger<GeneticAlgorithmBackgroundTaskQueue> logger)
        {
            _services = services;
            _notification = notification;
            _logger = logger;
        }

        public IEnumerable<IGeneticAlgorithmTaskInfo> TaskInfos =>
            _geneticAlgorithmTasks.Values.Select(task => task.Info);

        public IGeneticAlgorithmTaskInfo Register(
            Group @group,
            Dictionary<AssignmentObjective, double> coefficients,
            PopulationCapacity capacity)
        {
            var task = new GeneticAlgorithmTask(group, coefficients, capacity);
            task.Listener = _notification.Clients.Group(task.Info.Id);
            _geneticAlgorithmTasks.Add(task.Info.Id, task);
            _notification.Clients.All.Registered(task.Info);
            return task.Info;
        }

        public bool Remove(string taskId)
        {
            if (!_geneticAlgorithmTasks.ContainsKey(taskId)) return false;
            Stop(taskId);
            var task = _geneticAlgorithmTasks[taskId];
            _tasksQueue.Enqueue(async token =>
            {
                try
                {
                    await _notification.Clients.All.Removing(task.Info);
                    await task.Task;
                    _geneticAlgorithmTasks.Remove(task.Info.Id);
                    await _notification.Clients.All.Removed(task.Info);
                }
                catch
                {
                    // ignored
                }
            });
            _signal.Release();
            return true;
        }

        public bool Build(string taskId)
        {
            if (!_geneticAlgorithmTasks.ContainsKey(taskId)) return false;
            var task = _geneticAlgorithmTasks[taskId];
            _tasksQueue.Enqueue(async token =>
            {
                _logger.LogInformation($"Building {taskId}");
                await _notification.Clients.All.Building(taskId);
                _logger.LogInformation($"Building {taskId}: Clients Notified");
                var group = task.Info.Group;
                _logger.LogInformation($"Building {taskId}: Find Existing Repository");
                AssignmentDataRepository repository;
                var exists = _repositories.Any(repo => repo.Group.Id == group.Id);
                if (exists)
                {
                    _logger.LogInformation($"Building {taskId}: Existing Repository Found");
                    repository = _repositories.First(repo => repo.Group.Id == group.Id);
                }
                else
                {
                    _logger.LogInformation($"Building {taskId}: Generate Repository From Database");
                    using (var scope = _services.CreateScope())
                    {
                        var database = scope.ServiceProvider.GetRequiredService<AssignmentDatabase>();
                        repository = await AssignmentDataRepository.BuildAsync(database, group, token);
                        _repositories.Add(repository);   
                    }
                }

                _logger.LogInformation($"Building {taskId}: Building Task");
                task.Build(repository, exception =>
                {
                    _logger.LogError(exception, exception.Message);
                });
                if (task.Info.State == GeneticAlgorithmTaskState.BuildFailed)
                {
                    _logger.LogInformation($"Build Failed {taskId}");
                    await _notification.Clients.All.BuildFailed(taskId);
                }
                else
                {
                    _logger.LogInformation($"Build Completed {taskId}");
                    await _notification.Clients.All.BuildCompleted(taskId);
                }
            });
            _signal.Release();
            return true;
        }

        public bool Start(string taskId, TerminationKind kind, int value)
        {
            if (!_geneticAlgorithmTasks.ContainsKey(taskId)) return false;
            var task = _geneticAlgorithmTasks[taskId];
            _tasksQueue.Enqueue(async token =>
            {
                await _notification.Clients.All.Starting(taskId);
                task.Start(kind, value, (state, population) =>
                    {
                        _notification.Clients.All.Finished(taskId, state, population.Chromosomes);
                    });
                await _notification.Clients.All.Started(taskId);
            });
            _signal.Release();
            return true;
        }

        public bool Stop(string taskId)
        {
            if (!_geneticAlgorithmTasks.ContainsKey(taskId)) return false;
            var task = _geneticAlgorithmTasks[taskId];
            _tasksQueue.Enqueue(async token =>
            {
                await _notification.Clients.All.Stopping(taskId);
                task.Stop();
            });
            _signal.Release();
            return true;
        }

        public void Enqueue(Func<CancellationToken, Task> task)
        {
            _tasksQueue.Enqueue(task);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueTask(CancellationToken token)
        {
            await _signal.WaitAsync(token);
            _tasksQueue.TryDequeue(out var task);
            return task;
        }
    }
}