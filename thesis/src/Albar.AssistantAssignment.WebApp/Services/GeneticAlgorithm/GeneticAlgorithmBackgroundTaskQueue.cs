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

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public class GeneticAlgorithmBackgroundTaskQueue : IGeneticAlgorithmBackgroundTaskQueue
    {
        private readonly IServiceProvider _services;
        private readonly IHubContext<GeneticAlgorithmNotificationHub, IGeneticAlgorithmTaskListener> _notification;

        private readonly HashSet<AssignmentDataRepository> _repositories =
            new HashSet<AssignmentDataRepository>();

        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _tasksQueue =
            new ConcurrentQueue<Func<CancellationToken, Task>>();

        private readonly Dictionary<Guid, IGeneticAlgorithmTask> _geneticAlgorithmTasks =
            new Dictionary<Guid, IGeneticAlgorithmTask>();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public GeneticAlgorithmBackgroundTaskQueue(
            IServiceProvider services,
            IHubContext<GeneticAlgorithmNotificationHub, IGeneticAlgorithmTaskListener> notification)
        {
            _services = services;
            _notification = notification;
        }

        public IEnumerable<IGeneticAlgorithmTaskInfo> TaskInfos =>
            _geneticAlgorithmTasks.Values.Select(task => task.Info);

        public IGeneticAlgorithmTaskInfo Register(
            Group @group,
            Dictionary<AssignmentObjective, double> coefficients,
            PopulationCapacity capacity)
        {
            var task = new GeneticAlgorithmTask(group, coefficients, capacity);
            task.Listener = _notification.Clients.Group(task.Info.Id.ToString());
            _geneticAlgorithmTasks.Add(task.Info.Id, task);
            _notification.Clients.All.Registered(task.Info);
            return task.Info;
        }

        public bool Remove(Guid taskId)
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

        public bool Build(Guid taskId)
        {
            if (!_geneticAlgorithmTasks.ContainsKey(taskId)) return false;
            var task = _geneticAlgorithmTasks[taskId];
            _tasksQueue.Enqueue(async token =>
            {
                await _notification.Clients.All.Building(taskId);
                var group = task.Info.Group;
                var repository = _repositories.First(repo => repo.Group.Id == group.Id);
                if (repository == null)
                {
                    using (var scope = _services.CreateScope())
                    {
                        var database = scope.ServiceProvider.GetRequiredService<AssignmentDatabase>();
                        repository = await AssignmentDataRepository.BuildAsync(database, group, token);
                        _repositories.Add(repository);   
                    }
                }

                task.Build(repository);
                if (task.Info.State == GeneticAlgorithmTaskState.BuildFailed)
                {
                    await _notification.Clients.All.BuildFailed(taskId);
                }
                else
                {
                    await _notification.Clients.All.BuildCompleted(taskId);
                }
            });
            _signal.Release();
            return true;
        }

        public bool Start(Guid taskId, TerminationKind kind, int value)
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

        public bool Stop(Guid taskId)
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