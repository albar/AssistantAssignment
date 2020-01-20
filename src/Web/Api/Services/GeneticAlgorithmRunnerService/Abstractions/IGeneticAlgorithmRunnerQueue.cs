using System.Threading;
using System.Threading.Tasks;

namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions
{
    public interface IGeneticAlgorithmRunnerQueue
    {
        void Enqueue(IGeneticAlgorithmRunner runner);
        Task<IGeneticAlgorithmRunner> DequeueAsync(CancellationToken token);
    }
}
