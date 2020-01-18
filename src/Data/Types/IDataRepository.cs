using System.Collections.Immutable;

namespace AssistantAssignment.Data.Types
{
    public interface IDataRepository
    {
        int Id { get; }
        ImmutableArray<Schedule> Schedules { get; }
        ImmutableArray<Assistant> Assistants { get; }
        ImmutableArray<Course> Courses { get; }
    }
}
