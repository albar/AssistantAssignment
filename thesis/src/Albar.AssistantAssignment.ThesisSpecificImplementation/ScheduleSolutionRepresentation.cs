using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class ScheduleSolutionRepresentation : IScheduleSolutionRepresentation
    {
        public Schedule Schedule { get; set; }
        public AssistantCombination AssistantCombination { get; set; }
        ISchedule IScheduleSolutionRepresentation.Schedule => Schedule;
        IAssistantCombination IScheduleSolutionRepresentation.AssistantCombination => AssistantCombination;
    }
}