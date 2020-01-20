using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AssistantAssignment.Algorithm;
using AssistantAssignment.Algorithm.ObjectiveValueCalculators;
using AssistantAssignment.Algorithm.Reproductions;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions;
using EvolutionaryAlgorithm.Abstraction;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2.OffspringSelectors;

namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService
{
    public class GeneticAlgorithmRunnerBuilder : IGeneticAlgorithmRunnerBuilder
    {
        private static readonly Objectives[] OBJECTIVES =
            Enum.GetValues(typeof(Objectives)).Cast<Objectives>().ToArray();
        private readonly IDataRepository _repository;
        private int _size = 28;
        private IReproduction<Chromosome> _mutation;

        public GeneticAlgorithmRunnerBuilder(IDataRepository repository)
        {
            _repository = repository;
        }

        public IGeneticAlgorithmRunnerBuilder WithMutation(
            bool isWithMutation = true)
        {
            if (isWithMutation)
                _mutation = new Mutation(_repository);
            return this;
        }

        public IGeneticAlgorithmRunnerBuilder WithSize(int size)
        {
            if (size < 2)
                throw new Exception("Size at least should be 2");

            _size = size;
            return this;
        }

        public IGeneticAlgorithmRunner Build()
        {
            var evaluator = BuildEvaluator();
            var reinsertion = BuildReinsertion();

            var nsga = new NSGA2<Chromosome>(
                new Crossover(_repository),
                _mutation,
                evaluator,
                reinsertion);

            return new GeneticAlgorithmRunner(
                new ChromosomeFactory(_repository),
                nsga,
                evaluator,
                new EventHandler(),
                _size);
        }

        private IEvaluator<Chromosome> BuildEvaluator()
        {
            var phenotypeResolver = new PhenotypeResolver(_repository);
            var calculators = new ObjectiveValueCalculatorBase[]
            {
                new SchedulesCollisionCalculator(_repository),
                new AboveAverageCalculator(),
                new AboveThresholdCalculator(_repository),
                new NormalizedAssessmentsValuesCalculator(),
            }.ToImmutableHashSet();

            return new ChromosomeEvaluator(phenotypeResolver, calculators);
        }

        private IReinsertion<Chromosome> BuildReinsertion()
        {
            var selector = new NSGA2LastFrontOffspringSelector<Chromosome, Objectives, ObjectivesValue>(
                OBJECTIVES,
                new ObjectivesValueMapper());

            return new NSGAReinsertion<Chromosome, ObjectivesValue>(
                ObjectivesValue.DefaultComparer,
                selector);
        }

        internal class EventHandler : IGeneticAlgorithmRunnerEventHandler
        {
            public Task OnEvolvedOnce(
                string id,
                int generation,
                IEnumerable<Chromosome> pop)
            {
                throw new NotImplementedException();
            }

            public Task OnEvolving(string id)
            {
                throw new NotImplementedException();
            }

            public Task OnFinished(string id)
            {
                throw new NotImplementedException();
            }

            public Task OnInitialized(string id)
            {
                throw new NotImplementedException();
            }

            public Task OnInitializing(string id)
            {
                throw new NotImplementedException();
            }
        }
    }
}
