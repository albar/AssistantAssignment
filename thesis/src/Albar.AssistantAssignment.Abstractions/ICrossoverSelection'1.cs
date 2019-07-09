using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface ICrossoverSelection<T> where T : Enum
    {
        IEnumerable<(ImmutableArray<bool> Schema,
                IAssignmentChromosome<T> Parent1,
                IAssignmentChromosome<T> Parent2
                )>
            SelectCrossoverParent(IEnumerable<IAssignmentChromosome<T>> chromosomes, PopulationCapacity capacity);
    }
}