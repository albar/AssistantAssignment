using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class ScheduleSolutionRepresentation : IScheduleSolutionRepresentation
    {
        public ISchedule Schedule { get; set; }
        public IAssistantCombination AssistantCombination { get; set; }
    }
}