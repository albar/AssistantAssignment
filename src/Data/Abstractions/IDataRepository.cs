using System.Collections.Immutable;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Data.Abstractions
{
    public interface IDataRepository
    {
        int Id { get; }
        ImmutableArray<Schedule> Schedules { get; }
        ImmutableArray<Assistant> Assistants { get; }
        ImmutableArray<Course> Courses { get; }
    }
}
