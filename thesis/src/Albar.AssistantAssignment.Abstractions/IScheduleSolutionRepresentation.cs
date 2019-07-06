using Albar.AssistantAssignment.Data.Abstraction;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IScheduleSolutionRepresentation
    {
        ISchedule Schedule { get; }
        IAssistantCombination AssistantCombination { get; }
    }
}