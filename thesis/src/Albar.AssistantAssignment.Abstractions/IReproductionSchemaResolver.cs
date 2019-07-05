using System.Collections.Immutable;
using Bunnypro.GeneticAlgorithm.Abstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IReproductionSchemaResolver<in T> where T : IChromosome
    {
        ImmutableArray<bool> Resolve(params T[] chromosomes);
    }
}