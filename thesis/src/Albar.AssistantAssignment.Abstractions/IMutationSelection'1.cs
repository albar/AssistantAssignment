using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IMutationSelection<T> where T : Enum
    {
        IEnumerable<(ImmutableArray<bool> Schema, IAssignmentChromosome<T> Parent)>
            SelectMutationParent(IEnumerable<IAssignmentChromosome<T>> chromosomes, PopulationCapacity capacity);
    }
}