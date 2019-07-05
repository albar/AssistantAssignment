using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IAssignmentChromosome<T> : IChromosome<T> where T : Enum
    {
        new ImmutableArray<byte> Genotype { get; }
        new IEnumerable<IScheduleSolutionRepresentation> Phenotype { get; }
    }
}