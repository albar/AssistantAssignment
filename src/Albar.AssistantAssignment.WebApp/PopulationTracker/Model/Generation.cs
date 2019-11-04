using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Albar.AssistantAssignment.WebApp.PopulationTracker.Model
{
    public class Generation
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public RunningTask RunningTask { get; set; }

        [NotMapped] public IEnumerable<Chromosome> Chromosomes => GenerationChromosomes.Select(gn => gn.Chromosome);
        public IList<GenerationChromosome> GenerationChromosomes { get; set; }
    }
}