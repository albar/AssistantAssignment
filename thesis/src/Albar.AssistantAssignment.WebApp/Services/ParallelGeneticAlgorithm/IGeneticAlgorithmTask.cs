using System.Collections.Generic;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Models;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm
{
    public interface IGeneticAlgorithmTask
    {
        string Id { get; }
        Group Group { get; }
        IGeneticAlgorithm GeneticAlgorithm { get; }
        IPopulation Population { get; }
        IReadOnlyDictionary<AssignmentObjective, double> Coefficients { get; }
        PopulationCapacity Capacity { get; }
        bool IsRunning { get; }
        GeneticEvolutionStates EvolutionState { get; }
        GeneticAlgorithmTaskState State { get; }
        AssignmentDataRepository Repository { get; }
    }
}