using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Factories;
using Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators;
using Albar.AssistantAssignment.WebApp.Hubs;
using Albar.AssistantAssignment.WebApp.Models;
using Albar.AssistantAssignment.WebApp.PopulationTracker;
using Albar.AssistantAssignment.WebApp.Services.DatabaseTask;
using Albar.AssistantAssignment.WebApp.Services.GenericBackgroundTask;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.Core;
using Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2;
using Bunnypro.GeneticAlgorithm.Primitives;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm
{
    public class GeneticAlgorithmBackgroundTaskQueue : IGeneticAlgorithmBackgroundTaskQueue
    {
        private readonly IGenericBackgroundTaskQueue _genericBackgroundTaskQueue;
        private readonly IDatabaseBackgroundTaskQueue _databaseBackgroundTaskQueue;
        private readonly IGeneticAlgorithmTaskListener _notification;
        private readonly ILogger<GeneticAlgorithmBackgroundTaskQueue> _logger;

        private readonly HashSet<AssignmentDataRepository> _repositories = new HashSet<AssignmentDataRepository>();

        private readonly ConcurrentQueue<Func<string, CancellationTokenSource, Task<(GeneticEvolutionStates, bool)>>>
            _queue = new ConcurrentQueue<Func<string, CancellationTokenSource, Task<(GeneticEvolutionStates, bool)>>>();

        private readonly HashSet<GeneticAlgorithmTask> _tasks = new HashSet<GeneticAlgorithmTask>();

        private IEnumerable<GeneticAlgorithmRunningTask>
            RunningTasks => _tasks.Where(task => task.RunningTask != null).Select(task => task.RunningTask);

        private readonly SemaphoreSlim _sign = new SemaphoreSlim(0);

        public GeneticAlgorithmBackgroundTaskQueue(
            IGenericBackgroundTaskQueue genericBackgroundTaskQueue,
            IDatabaseBackgroundTaskQueue databaseBackgroundTaskQueue,
            IHubContext<GeneticAlgorithmNotificationHub, IGeneticAlgorithmTaskListener> notification,
            ILogger<GeneticAlgorithmBackgroundTaskQueue> logger)
        {
            _genericBackgroundTaskQueue = genericBackgroundTaskQueue;
            _databaseBackgroundTaskQueue = databaseBackgroundTaskQueue;
            _notification = notification.Clients.All;
            _logger = logger;
        }

        public IEnumerable<IGeneticAlgorithmTask> Tasks => _tasks;

        public IGeneticAlgorithmTask Build(
            Group group,
            Dictionary<AssignmentObjective, double> coefficients,
            PopulationCapacity capacity)
        {
            var id = Guid.NewGuid().ToString();
            _logger.LogInformation($"Enqueue for Build Task {id}");
            var task = new GeneticAlgorithmTask(id, group, coefficients, capacity);
            _tasks.Add(task);
            _databaseBackgroundTaskQueue.Enqueue(async (database, token) =>
            {
                var repository = _repositories.FirstOrDefault(repo => repo.Group.Id == group.Id);
                if (repository == null)
                {
                    _logger.LogInformation($"Building Repository for Task {id}");
                    await _notification.BuildingRepositoryTask(id);
                    task.State = GeneticAlgorithmTaskState.BuildingRepositoryTask;

                    repository = await AssignmentDataRepository.BuildAsync(database, group, token);
                    _repositories.Add(repository);
                }

                await _notification.BuildingTask(id);
                task.State = GeneticAlgorithmTaskState.BuildingTask;

                task.Build(repository);
                _logger.LogInformation($"Task {id} has been Build");


                await _notification.TaskBuildFinished(id, new
                {
                    SubjectCount = task.Repository.Subjects.Count,
                    ScheduleCount = task.Repository.Schedules.Count,
                    AssistantCount = task.Repository.Assistants.Count
                });
                task.State = GeneticAlgorithmTaskState.TaskBuildFinished;
            });

            return task;
        }

        public bool Remove(string taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null || task.RunningTask != null) return false;
            _tasks.Remove(task);
            _logger.LogInformation($"Task {taskId} Removed");
            _notification.TaskRemoved(taskId);
            return true;
        }

        public bool Start(string taskId, Func<GeneticEvolutionStates, bool> termination)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null || task.IsRunning) return false;

            _logger.LogInformation($"Enqueue Running Task {taskId}");
            _queue.Enqueue((id, source) =>
            {
                _logger.LogInformation($"Running Task {taskId}. RunningId: {id}");
                _notification.RunningTask(taskId);
                task.State = GeneticAlgorithmTaskState.RunningTask;
                task.RunningTask = new GeneticAlgorithmRunningTask(taskId, id, source);
                var ga = task.GeneticAlgorithm;
                task.Population = task.PopulationFactory.Create(task.Capacity, new EvolutionTracker(id, _genericBackgroundTaskQueue));
                var population = task.Population;

                bool MonitoredTermination(GeneticEvolutionStates state)
                {
                    var chromosome = population.Chromosomes.Cast<AssignmentChromosome<AssignmentObjective>>()
                        .OrderBy(ch => ch.Fitness)
                        .Last();

                    _notification.EvolvedOnce(taskId, new
                    {
                        state.EvolutionCount,
                        state.EvolutionTime,
                        chromosomeCount = population.Chromosomes.Count
                    }, new
                    {
                        chromosome.Fitness,
                        chromosome.ObjectiveValues
                    });
                    task.EvolutionState = state;
                    return termination.Invoke(state);
                }

                _notification.TaskIsRunning(taskId);
                return ga.TryEvolveUntil(population, MonitoredTermination, source.Token);
            });
            _sign.Release();
            return true;
        }

        public bool Stop(string taskId)
        {
            _logger.LogInformation($"Stopping Task {taskId}");
            _notification.StoppingTask(taskId);
            var runningTask = RunningTasks.FirstOrDefault(rtask => rtask.TaskId == taskId);
            runningTask?.TokenSource?.Cancel();
            return runningTask != null;
        }

        public async Task<Func<string, CancellationTokenSource, Task<(GeneticEvolutionStates, bool)>>> DequeueAsync(
            CancellationToken token)
        {
            await _sign.WaitAsync(token);
            _queue.TryDequeue(out var task);
            return task;
        }

        public void BackgroundTaskFinished(string runningTaskId, (GeneticEvolutionStates, bool) result)
        {
            var task = _tasks.FirstOrDefault(t => t.RunningTask?.RunningTaskId == runningTaskId);
            if (task == null) return;
            task.RunningTask = null;
            _logger.LogInformation($"Running Task {task.Id} with RunningId: {runningTaskId} Finished");
            _notification.TaskFinished(task.Id, new
            {
                result.Item1.EvolutionCount,
                result.Item1.EvolutionTime,
                chromosomeCount = task.Population.Chromosomes.Count
            });
            task.State = GeneticAlgorithmTaskState.TaskFinished;
        }

        public void BackgroundTaskFailed(string runningTaskId)
        {
            var task = _tasks.FirstOrDefault(t => t.RunningTask?.RunningTaskId == runningTaskId);
            if (task == null) return;
            task.RunningTask = null;
            _logger.LogInformation($"Running Task {task.Id} with RunningId: {runningTaskId} Failed");
            _notification.TaskFailed(task.Id);
            task.State = GeneticAlgorithmTaskState.TaskFailed;
        }

        private class GeneticAlgorithmTask : IGeneticAlgorithmTask, IEquatable<GeneticAlgorithmTask>
        {
            public GeneticAlgorithmTask(
                string id,
                Group group,
                IReadOnlyDictionary<AssignmentObjective, double> coefficients,
                PopulationCapacity capacity)
            {
                Id = id;
                Group = group;
                Coefficients = coefficients;
                Capacity = capacity;
            }

            public void Build(AssignmentDataRepository repository)
            {
                var genotypePhenotypeMapper = new GenotypePhenotypeMapper(repository);

                var reproduction = new AssignmentReproduction<AssignmentObjective>(
                    genotypePhenotypeMapper,
                    new ReproductionSelection(repository)
                );
                var objectiveEvaluator = new AssignmentChromosomesEvaluator<AssignmentObjective>(Coefficients)
                {
                    {AssignmentObjective.AssistantScheduleCollision, new AssistantScheduleCollisionEvaluator()},
                    {AssignmentObjective.AboveThresholdAssessment, new AboveThresholdAssessmentEvaluator(repository)},
                    {AssignmentObjective.BelowThresholdAssessment, new BelowThresholdAssessmentEvaluator(repository)},
                    {
                        AssignmentObjective.AverageOfNormalizedAssessment,
                        new AverageOfNormalizedAssessmentEvaluator(repository)
                    }
                };
                var nsga = new NSGA2<AssignmentObjective>(
                    reproduction,
                    objectiveEvaluator,
                    Coefficients
                );
                PopulationFactory =
                    new PopulationFactory<AssignmentObjective>(genotypePhenotypeMapper, objectiveEvaluator);
                GeneticAlgorithm = new GeneticAlgorithm(nsga);
                Repository = repository;
            }

            public AssignmentDataRepository Repository { get; private set; }
            public string Id { get; }
            public Group Group { get; }
            public IGeneticAlgorithm GeneticAlgorithm { get; private set; }
            public IPopulation Population { get; set; }
            public IReadOnlyDictionary<AssignmentObjective, double> Coefficients { get; }
            public PopulationCapacity Capacity { get; }
            public bool IsRunning => RunningTask != null;
            public GeneticEvolutionStates EvolutionState { get; set; }
            public GeneticAlgorithmTaskState State { get; set; }
            public GeneticAlgorithmRunningTask RunningTask { get; set; }
            public PopulationFactory<AssignmentObjective> PopulationFactory { get; set; }

            public bool Equals(GeneticAlgorithmTask other)
            {
                if (ReferenceEquals(null, other)) return false;
                return ReferenceEquals(this, other) || string.Equals(Id, other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((GeneticAlgorithmTask) obj);
            }

            public override int GetHashCode()
            {
                return Id != null ? Id.GetHashCode() : 0;
            }
        }

        private class GeneticAlgorithmRunningTask
        {
            public GeneticAlgorithmRunningTask(
                string taskId,
                string runningTaskId,
                CancellationTokenSource tokenSource)
            {
                TaskId = taskId;
                RunningTaskId = runningTaskId;
                TokenSource = tokenSource;
            }

            public string TaskId { get; }
            public string RunningTaskId { get; }
            public CancellationTokenSource TokenSource { get; }
        }
    }
}