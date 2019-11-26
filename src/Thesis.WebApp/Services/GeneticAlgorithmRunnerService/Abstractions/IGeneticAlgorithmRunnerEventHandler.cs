using System.Collections.Generic;
using System.Threading.Tasks;
using Thesis.Algorithm;

namespace Thesis.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions
{
    public interface IGeneticAlgorithmRunnerEventHandler
    {
        Task OnInitializing(string id);
        Task OnInitialized(string id);
        Task OnEvolving(string id);
        Task OnEvolvedOnce(string id, int generation, IEnumerable<Chromosome> pop);
        Task OnFinished(string id);
    }
}
