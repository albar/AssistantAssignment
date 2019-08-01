using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IDataRepository
    {
        byte GeneByteSize { get; }
        ImmutableDictionary<int, ISubject> Subjects { get; }
        ImmutableDictionary<int, ISchedule> Schedules { get; }
        ImmutableDictionary<int, IAssistant> Assistants { get; }
        ImmutableDictionary<int, IAssistantCombination> AssistantCombinations { get; }
    }
}