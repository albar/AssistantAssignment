using System.Collections.Generic;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Models;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public interface IGeneticAlgorithmTaskInfo
    {
        GeneticAlgorithmTaskState State { get; }
        string Id { get; }
        Group Group { get; }
        PopulationCapacity Capacity { get; }
        IReadOnlyDictionary<AssignmentObjective, double> Coefficients { get; }
    }
}