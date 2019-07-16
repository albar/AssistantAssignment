using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Models;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm
{
    public interface IGeneticAlgorithmBackgroundTaskQueue
    {
        IEnumerable<IGeneticAlgorithmTask> Tasks { get; }

        IGeneticAlgorithmTask Build(
            Group group,
            Dictionary<AssignmentObjective, double> coefficients,
            PopulationCapacity capacity);

        bool Remove(string taskId);

        bool Start(string taskId, Func<GeneticEvolutionStates, bool> termination);

        bool Stop(string taskId);

        Task<Func<string, CancellationTokenSource, Task<(GeneticEvolutionStates, bool)>>> DequeueAsync(
            CancellationToken token);

        void BackgroundTaskFinished(string runningTaskId, (GeneticEvolutionStates, bool) result);
    }
}