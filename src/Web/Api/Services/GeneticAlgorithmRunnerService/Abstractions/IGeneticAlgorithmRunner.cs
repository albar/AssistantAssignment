using System.Threading;
using System.Threading.Tasks;

namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions
{
    public interface IGeneticAlgorithmRunner
    {
        string Id { get; }

        Task StartAsync(CancellationToken token);
        Task StopAsync(CancellationToken token);
    }
}
