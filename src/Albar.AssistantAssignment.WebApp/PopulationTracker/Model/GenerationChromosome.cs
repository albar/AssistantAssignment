namespace Albar.AssistantAssignment.WebApp.PopulationTracker.Model
{
    public class GenerationChromosome
    {
        public int GenerationId { get; set; }
        public int ChromosomeId { get; set; }
        
        public Generation Generation { get; set; }
        public Chromosome Chromosome { get; set; }
    }
}