using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IScheduleSolutionRepresentation
    {
        ISchedule Schedule { get; }
        IAssistantCombination AssistantCombination { get; }
    }
}