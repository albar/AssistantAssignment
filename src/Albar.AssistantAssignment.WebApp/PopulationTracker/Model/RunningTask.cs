using System.Collections.Generic;

namespace Albar.AssistantAssignment.WebApp.PopulationTracker.Model
{
    public class RunningTask
    {
        public int Id { get; set; }
        public string TaskId { get; set; }
        public IList<Generation> Generations { get; set; }
        public IList<Chromosome> Chromosomes { get; set; }
    }
}