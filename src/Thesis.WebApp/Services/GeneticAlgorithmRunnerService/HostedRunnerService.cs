using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Thesis.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace Thesis.WebApp.Services.GeneticAlgorithmRunnerService
{
    public class HostedRunnerService : BackgroundService
    {
        private readonly IGeneticAlgorithmRunnerQueue _queue;

        public HostedRunnerService(IGeneticAlgorithmRunnerQueue queue)
        {
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var stoppers = new List<Func<CancellationToken, Task>>();

            while (!token.IsCancellationRequested)
            {
                var runner = await _queue.DequeueAsync(token);
                await runner.StartAsync(token);
                stoppers.Add(runner.StopAsync);
            }

            await Task.WhenAll(stoppers.Select(stopper => stopper.Invoke(token)));
        }
    }
}
