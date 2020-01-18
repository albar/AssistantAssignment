using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService
{
    public class GeneticAlgorithmBuilderQueue : IGeneticAlgorithmBuilderQueue
    {
        private readonly ConcurrentQueue<IGeneticAlgorithmBuilder> _queue =
            new ConcurrentQueue<IGeneticAlgorithmBuilder>();

        private readonly SemaphoreSlim _sign = new SemaphoreSlim(0);

        public string EnqueueBuilder(Action<IGeneticAlgorithmBuilder> config)
        {
            var builder = new GeneticAlgorithmBuilder();
            config.Invoke(builder);
            _queue.Enqueue(builder);
            _sign.Release();
            return builder.Id;
        }

        public async Task<IGeneticAlgorithmBuilder> DequeueAsync(CancellationToken token)
        {
            await _sign.WaitAsync(token);
            _queue.TryDequeue(out var builder);
            return builder;
        }
    }
}
