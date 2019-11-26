using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thesis.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions
{
    public interface IGeneticAlgorithmBuilderQueue
    {
        string EnqueueBuilder(Action<IGeneticAlgorithmBuilder> config);
        Task<IGeneticAlgorithmBuilder> DequeueAsync(CancellationToken token);
    }
}
