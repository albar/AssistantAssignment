using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Models;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public interface IGeneticAlgorithmBackgroundTaskQueue
    {
        IEnumerable<IGeneticAlgorithmTaskInfo> TaskInfos { get; }
        IGeneticAlgorithmTaskInfo Register(
            Group @group,
            Dictionary<AssignmentObjective, double> coefficients,
            PopulationCapacity capacity);

        bool Remove(string taskId);
        bool Build(string taskId);
        bool Start(string taskId, TerminationKind kind, int value);
        bool Stop(string taskId);
        void Enqueue(Func<CancellationToken, Task> task);
        Task<Func<CancellationToken, Task>> DequeueTask(CancellationToken token);
    }
}