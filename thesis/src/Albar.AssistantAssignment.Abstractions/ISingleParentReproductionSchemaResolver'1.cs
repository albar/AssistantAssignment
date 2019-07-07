using System.Collections.Immutable;
using Bunnypro.GeneticAlgorithm.Abstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface ISingleParentReproductionSchemaResolver<in T> where T : IChromosome
    {
        ImmutableArray<bool> Resolve(T chromosome);
    }
}