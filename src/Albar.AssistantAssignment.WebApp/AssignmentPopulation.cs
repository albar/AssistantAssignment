using System.Collections.Immutable;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.Core;

namespace Albar.AssistantAssignment.WebApp
{
    public class AssignmentPopulation : Population
    {
        private readonly IPopulationEventHandler _eventHandler;

        public AssignmentPopulation(IPopulationEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
        }
        
        public override ImmutableHashSet<IChromosome> Chromosomes
        {
            get => base.Chromosomes;
            set
            {
                base.Chromosomes = value;
                _eventHandler.OnChromosomesUpdated(base.Chromosomes, GenerationNumber);
            }
        }
    }
}