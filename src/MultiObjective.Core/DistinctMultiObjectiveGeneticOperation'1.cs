using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Core
{
    public abstract class DistinctMultiObjectiveGeneticOperation<T> : IGeneticOperation, IDistinctMultiObjectiveGeneticOperation<T> where T : Enum
    {
        public async Task<ImmutableHashSet<IChromosome>> Operate(ImmutableHashSet<IChromosome> chromosomes, PopulationCapacity capacity, CancellationToken token = default)
        {
            var offspring = await Operate(chromosomes.Cast<IChromosome<T>>(), capacity, token);
            return offspring.Cast<IChromosome>().ToImmutableHashSet();
        }

        public abstract Task<ImmutableHashSet<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> parents,
            PopulationCapacity capacity,
            CancellationToken token = default);
    }
}