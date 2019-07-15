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

        bool Remove(Guid taskId);
        bool Build(Guid taskId);
        bool Start(Guid taskId, TerminationKind kind, int value);
        bool Stop(Guid taskId);
        void Enqueue(Func<CancellationToken, Task> task);
        Task<Func<CancellationToken, Task>> DequeueTask(CancellationToken token);
    }
}