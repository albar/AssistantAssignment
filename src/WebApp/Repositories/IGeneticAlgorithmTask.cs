using System.Collections.Generic;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace AssistantAssignment.WebApp.Repositories
{
    public interface IGeneticAlgorithmTask
    {
        IDictionary<string, IGeneticAlgorithmRunner> Runners { get; }
    }
}
