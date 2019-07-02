using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions
{
    public interface IDistinctMultiObjectiveGeneticOperation<T> where T : Enum
    {
         Task<ImmutableHashSet<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> parents,
            PopulationCapacity capacity,
            CancellationToken token = default);
    }
}