using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;
using AssistantAssignment.Algorithm;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService
{
    public class GeneticAlgorithmRunner : IGeneticAlgorithmRunner
    {
        private readonly NSGA2<Chromosome, Objectives> _nsga;
        private readonly IGeneticAlgorithmRunnerEventHandler _eventHandler;
        private readonly ChromosomeEvaluator _evaluator;
        private readonly ChromosomeFactory _factory;
        private CancellationTokenSource _cts;
        private Task _runningTask;

        private ImmutableHashSet<Chromosome> _pop;
        private int _generation;

        public GeneticAlgorithmRunner(ChromosomeFactory factory,
            NSGA2<Chromosome, Objectives> nsga,
            ChromosomeEvaluator evaluator,
            IGeneticAlgorithmRunnerEventHandler eventHandler,
            int size)
        {
            _factory = factory;
            _nsga = nsga;
            _evaluator = evaluator;
            _eventHandler = eventHandler;
            Size = size;
        }

        public string Id { get; } = Guid.NewGuid().ToString();
        public int Size { get; set; }

        public async Task StartAsync(CancellationToken token)
        {
            await InitializeAsync(token);
            await _eventHandler.OnEvolving(Id);
            _cts = new CancellationTokenSource();
            _runningTask = _nsga.EvolveAsync(_pop, _cts.Token);
        }

        public async Task StopAsync(CancellationToken token)
        {
            if (_cts == null)
            {
                return;
            }

            try
            {
                _cts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_runningTask, Task.Delay(Timeout.Infinite, token));
                await _eventHandler.OnFinished(Id);
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task InitializeAsync(CancellationToken token)
        {
            await _eventHandler.OnInitializing(Id);
            _generation = 0;
            _pop = await _factory.CreateAsync(Size, token);
            await _evaluator.EvaluateAsync(_pop, token);
            _nsga.ExpectedResultCount = Size;
            _nsga.OnEvolvedOnce += offspring =>
            {
                _pop = offspring;
                _generation++;
                _eventHandler.OnEvolvedOnce(Id, _generation, _pop).ConfigureAwait(false);
            };
            await _eventHandler.OnInitialized(Id);
        }
    }
}
