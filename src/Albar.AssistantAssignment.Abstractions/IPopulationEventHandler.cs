using System.Collections.Immutable;
using Bunnypro.GeneticAlgorithm.Abstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IPopulationEventHandler
    {
        void OnChromosomesUpdated(ImmutableHashSet<IChromosome> chromosomes, int generationNumber);
    }
}