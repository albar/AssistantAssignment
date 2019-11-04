using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Albar.AssistantAssignment.ThesisSpecificImplementation;

namespace Albar.AssistantAssignment.WebApp.PopulationTracker.Model
{
    public class Chromosome
    {
        public int Id { get; set; }
        [NotMapped] public IEnumerable<Generation> Generations => GenerationChromosomes.Select(gn => gn.Generation);

        public IList<GenerationChromosome> GenerationChromosomes { get; set; }
        public string Genotype { get; set; }
        public double Fitness { get; set; }
        public IReadOnlyDictionary<AssignmentObjective, double> ObjectiveValues { get; set; }
        public int RunningTaskId { get; set; }
        public RunningTask RunningTask { get; set; }
    }
}