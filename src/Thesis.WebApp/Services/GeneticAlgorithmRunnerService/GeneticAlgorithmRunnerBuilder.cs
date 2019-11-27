using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using EvolutionaryAlgorithm.Abstraction;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;
using Thesis.Algorithm;
using Thesis.Algorithm.ObjectiveValueCalculators;
using Thesis.Algorithm.Reproductions;
using Thesis.DataType;
using Thesis.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace Thesis.WebApp.Services.GeneticAlgorithmRunnerService
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
            var reinsertion = new EuclideanBasedOffspringSelector<Chromosome, Objectives, ObjectivesValue>(
                Enum.GetValues(typeof(Objectives)).Cast<Objectives>(),
                new ObjectivesValueMapper(),
                ObjectivesValue.ObjectivesValueComparer);

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
