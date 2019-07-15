using System;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public interface IGeneticAlgorithmTask
    {
        IGeneticAlgorithmTaskInfo Info { get; }
        Task<GeneticEvolutionStates> Task { get; }
        void Build(IDataRepository<AssignmentObjective> repository, Action<Exception> onError);
        void Start(TerminationKind kind, int value,
            Action<GeneticEvolutionStates, IPopulation> onFinished);
        void Stop();
        void Reset();
    }
}