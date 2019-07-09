using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.Core;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Algorithm.Factories
{
    public class PopulationFactory<T> where T : Enum
    {
        private readonly IDataRepository _repository;

        public PopulationFactory(IDataRepository repository)
        {
            _repository = repository;
        }

        public IPopulation Create(PopulationCapacity capacity)
        {
            var chromosomes = ImmutableHashSet.CreateBuilder<IChromosome>();
            var randomize = new Random();
            while (chromosomes.Count < capacity.Minimum)
            {
                var genotype = _repository.Schedules.SelectMany(schedule =>
                    _repository.AssistantCombinations
                        .Where(c => c.Subject == schedule.Subject)
                        .OrderBy(_ => randomize.Next())
                        .First().Id
                );
                chromosomes.Add(new AssignmentChromosome<T>(genotype.ToImmutableArray()));
            }

            return new Population
            {
                Capacity = capacity,
                Chromosomes = chromosomes.ToImmutable()
            };
        }
    }
}