using System.Collections.Immutable;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.Core;

namespace Albar.AssistantAssignment.WebApp
{
    public class AssignmentPopulation : Population
    {
        public override ImmutableHashSet<IChromosome> Chromosomes
        {
            get => base.Chromosomes;
            set
            {
                base.Chromosomes = value;
                EventHandler?.OnChromosomesUpdated(base.Chromosomes, GenerationNumber);
            }
        }

        public IPopulationEventHandler? EventHandler { get; set; }
    }
}