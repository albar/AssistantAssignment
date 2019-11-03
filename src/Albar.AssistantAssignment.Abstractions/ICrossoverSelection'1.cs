using System;
using System.Collections.Generic;
using Albar.AssistantAssignment.Abstractions.Primitives;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface ICrossoverSelection<T> where T : Enum
    {
        IEnumerable<PreparedCrossoverParent<T>> SelectCrossoverParent(IEnumerable<IAssignmentChromosome<T>> chromosomes,
            PopulationCapacity capacity);
    }
}