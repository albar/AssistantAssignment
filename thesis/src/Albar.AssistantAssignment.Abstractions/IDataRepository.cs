using System.Collections.Immutable;
using Albar.AssistantAssignment.Data.Abstraction;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IDataRepository
    {
        int GeneSize { get; }
        ImmutableArray<ISchedule> Schedules { get; }
        ImmutableArray<ISubject> Subjects { get; }
        ImmutableArray<IAssistant> Assistants { get; }
        ImmutableArray<IAssistantCombination> AssistantCombinations { get; }
    }
}