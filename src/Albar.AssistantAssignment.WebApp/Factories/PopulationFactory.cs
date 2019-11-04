using System;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Services.DatabaseTask;
using Albar.AssistantAssignment.WebApp.Services.GenericBackgroundTask;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Factories
{
    public class PopulationFactory<T> where T : Enum
    {
        private readonly IGenotypePhenotypeMapper<T> _mapper;
        private readonly IChromosomeEvaluator<T> _evaluator;

        public PopulationFactory(IGenotypePhenotypeMapper<T> mapper, IChromosomeEvaluator<T> evaluator)
        {
            _mapper = mapper;
            _evaluator = evaluator;
        }

        public IPopulation Create(PopulationCapacity capacity)
        {
            var chromosomes = Create(capacity.Minimum);
            _evaluator.EvaluateAll(chromosomes.Cast<IChromosome<T>>()).Wait();

            return new AssignmentPopulation
            {
                Capacity = capacity,
                Chromosomes = chromosomes.Cast<IChromosome>().ToImmutableHashSet()
            };
        }

        public ImmutableHashSet<AssignmentChromosome<AssignmentObjective>> Create(int count)
        {
            var chromosomes = ImmutableHashSet.CreateBuilder<AssignmentChromosome<AssignmentObjective>>();
            var randomize = new Random();
            while (chromosomes.Count < count)
            {
                var genotype = _mapper.DataRepository.Schedules.SelectMany(schedule =>
                {
                    var id = _mapper.DataRepository.AssistantCombinations
                        .Select(combination => combination.Value)
                        .Where(c => c.Subject.Equals(schedule.Value.Subject))
                        .OrderBy(_ => randomize.Next())
                        .First().Id;
                    return ByteConverter.GetByte(_mapper.DataRepository.GeneByteSize, id);
                });
                var chromosome = new AssignmentChromosome<AssignmentObjective>(genotype.ToImmutableArray());
                if (chromosomes.Add(chromosome))
                    chromosome.Phenotype = _mapper.ToSolution(chromosome.Genotype.ToArray()).ToArray();
            }

            return chromosomes.ToImmutable();
        }
    }
}