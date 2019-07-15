using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm;
using Microsoft.Extensions.Hosting;

namespace Albar.AssistantAssignment.WebApp.Services
{
    public class QueuedGeneticAlgorithmBackgroundTask : BackgroundService
    {
        private readonly IGeneticAlgorithmBackgroundTaskQueue _queue;

        public QueuedGeneticAlgorithmBackgroundTask(IGeneticAlgorithmBackgroundTaskQueue queue)
        {
            _queue = queue;
        }
        
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var task = await _queue.DequeueTask(token);
                await task.Invoke(token);
            }
        }
    }
}