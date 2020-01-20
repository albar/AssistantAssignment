using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Algorithm;
using AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions;
using EvolutionaryAlgorithm.Abstraction;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;

namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService
{
    public class GeneticAlgorithmRunner : IGeneticAlgorithmRunner
    {
        private readonly NSGA2<Chromosome> _nsga;
        private readonly IGeneticAlgorithmRunnerEventHandler _eventHandler;
        private readonly IEvaluator<Chromosome> _evaluator;
        private readonly ChromosomeFactory _factory;
        private CancellationTokenSource _cts;
        private Task _runningTask;

        private ImmutableHashSet<Chromosome> _pop;
        private int _generation;

        public GeneticAlgorithmRunner(ChromosomeFactory factory,
            NSGA2<Chromosome> nsga,
            IEvaluator<Chromosome> evaluator,
            IGeneticAlgorithmRunnerEventHandler eventHandler,
            int size)
        {
            _factory = factory;
            _nsga = nsga;
            _evaluator = evaluator;
            _eventHandler = eventHandler;
            Size = size;
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; }
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
                return;

            try
            {
                _cts.Cancel();
                await Task.WhenAny(
                    _runningTask,
                    Task.Delay(Timeout.Infinite, token));
            }
            finally
            {
                await _eventHandler.OnFinished(Id);
                _cts.Dispose();
                _cts = null;
                if (_runningTask.IsCompleted)
                    _runningTask = null;
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
