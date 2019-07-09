using System;
using System.Collections.Generic;
using Albar.AssistantAssignment.Abstractions.Primitives;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IMutationSelection<T> where T : Enum
    {
        IEnumerable<PreparedMutationParent<T>> SelectMutationParent(
            IEnumerable<IAssignmentChromosome<T>> chromosomes,
            PopulationCapacity capacity);
    }
}