using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using EvolutionaryAlgorithm.Abstraction;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2.OffspringSelectors;
using AssistantAssignment.Algorithm;
using AssistantAssignment.Algorithm.ObjectiveValueCalculators;
using AssistantAssignment.Algorithm.Reproductions;
using AssistantAssignment.Data.Types;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService
{
    public class GeneticAlgorithmRunnerBuilder : IGeneticAlgorithmRunnerBuilder
    {
        private readonly IDataRepository _repository;
        private readonly ChromosomeFactory _factory;
        private int? _size = null;
        private IReproduction<Chromosome> _mutation;
        private ChromosomeEvaluator _evaluator;

        public GeneticAlgorithmRunnerBuilder(IDataRepository repository,
            ChromosomeFactory factory)
        {
            _repository = repository;
            _factory = factory;
        }

        public IGeneticAlgorithmRunner Build()
        {
            BuildDependencies();
            Validate();

            return new GeneticAlgorithmRunner(_factory, BuildNsga2(), _evaluator, new EventHandler(), (int)_size);
        }

        private void BuildDependencies()
        {
            var phenotypeResolver = new PhenotypeResolver(_repository);
            var calculators = new ObjectiveValueCalculatorBase[]
            {
                new SchedulesCollisionCalculator(_repository),
                new AboveAverageCalculator(_repository),
                new AboveThresholdCalculator(_repository),
                new NormalizedAssessmentsValuesCalculator(),
            }.ToImmutableHashSet();
            _evaluator = new ChromosomeEvaluator(phenotypeResolver, calculators);
        }

        private void Validate()
        {
            if (_size == null)
            {
                throw new System.Exception("Size could not be null");
            }

            if (_size < 2)
            {
                throw new System.Exception("Size should at least be 2");
            }
        }

        public IGeneticAlgorithmRunnerBuilder WithMutation(bool isWithMutation = false)
        {
            if (isWithMutation)
                _mutation = new Mutation(_repository);
            return this;
        }

        public IGeneticAlgorithmRunnerBuilder WithSize(int size)
        {
            _size = size;
            return this;
        }

        private NSGA2<Chromosome, Objectives> BuildNsga2()
        {
            var selector = new NSGA2LastFrontOffspringSelector<Chromosome, Objectives, ObjectivesValue>(
                Enum.GetValues(typeof(Objectives)).Cast<Objectives>(),
                new ObjectivesValueMapper());
            var reinsertion = new NSGAReinsertion<Chromosome, ObjectivesValue>(
                ObjectivesValue.ObjectivesValueComparer, selector);

            return new NSGA2<Chromosome, Objectives>(
                new Crossover(_repository),
                _mutation,
                _evaluator,
                reinsertion);
        }

        private class EventHandler : IGeneticAlgorithmRunnerEventHandler
        {
            public Task OnEvolvedOnce(string id, int generation, IEnumerable<Chromosome> pop)
            {
                throw new System.NotImplementedException();
            }

            public Task OnEvolving(string id)
            {
                throw new System.NotImplementedException();
            }

            public Task OnFinished(string id)
            {
                throw new System.NotImplementedException();
            }

            public Task OnInitialized(string id)
            {
                throw new System.NotImplementedException();
            }

            public Task OnInitializing(string id)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
