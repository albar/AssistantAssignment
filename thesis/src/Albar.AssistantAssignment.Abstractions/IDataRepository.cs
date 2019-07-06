using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IDataRepository
    {
        byte GeneSize { get; }
        ImmutableArray<ISubject> Subjects { get; }
        ImmutableArray<ISchedule> Schedules { get; }
        ImmutableArray<IAssistant> Assistants { get; }
        ImmutableArray<IAssistantCombination> AssistantCombinations { get; }
    }
}