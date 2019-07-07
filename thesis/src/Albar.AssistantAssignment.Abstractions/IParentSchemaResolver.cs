using Bunnypro.GeneticAlgorithm.Abstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IParentSchemaResolver<T> :
        ISingleParentReproductionSchemaResolver<T>,
        IMultiParentReproductionSchemaResolver<T>
        where T : IChromosome
    {
    }
}