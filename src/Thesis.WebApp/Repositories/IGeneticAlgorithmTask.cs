using System.Collections.Generic;
using Thesis.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace Thesis.WebApp.Repositories
{
    public interface IGeneticAlgorithmTask
    {
        IDictionary<string, IGeneticAlgorithmRunner> Runners { get; }
    }
}
