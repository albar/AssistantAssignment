using System.Collections.Immutable;

namespace Bunnypro.GeneticAlgorithm.Abstractions
{
    public interface IChromosome
    {
        double Fitness { get; }
        ImmutableArray<object> Gene { get; }
    }
}