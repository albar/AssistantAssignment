using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService
{
    public class GeneticAlgorithmRunnerQueue : IGeneticAlgorithmRunnerQueue
    {
        private readonly ConcurrentQueue<IGeneticAlgorithmRunner> _queue =
            new ConcurrentQueue<IGeneticAlgorithmRunner>();

        private readonly SemaphoreSlim _sign = new SemaphoreSlim(0);

        public void Enqueue(IGeneticAlgorithmRunner runner)
        {
            _queue.Enqueue(runner);
            _sign.Release();
        }

        public async Task<IGeneticAlgorithmRunner> DequeueAsync(CancellationToken token)
        {
            await _sign.WaitAsync(token);
            _queue.TryDequeue(out var runner);
            return runner;
        }
    }
}
