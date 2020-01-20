using System.Collections.Generic;
using AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace AssistantAssignment.Web.Api.Repositories
{
    public interface IGeneticAlgorithmTask
    {
        IReadOnlyDictionary<string, IGeneticAlgorithmRunner> Runners { get; }

        IGeneticAlgorithmRunnerBuilder CreateRunnerBuilder();
        IGeneticAlgorithmRunner AddRunnerBuilder(IGeneticAlgorithmRunnerBuilder builder);
    }
}
