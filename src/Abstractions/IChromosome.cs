using System;
using System.Collections.Immutable;

namespace Bunnypro.GeneticAlgorithm.Abstractions
{
    public interface IChromosome : IEquatable<IChromosome>
    {
        double Fitness { get; set; }
        ImmutableArray<object> Genotype { get; }
        object Phenotype { get; }
    }
}