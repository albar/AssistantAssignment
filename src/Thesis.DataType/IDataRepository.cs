using System.Collections.Immutable;

namespace Thesis.DataType
{
    public interface IDataRepository
    {
        int Id { get; }
        ImmutableArray<Schedule> Schedules { get; }
        ImmutableArray<Assistant> Assistants { get; }
        ImmutableArray<Course> Courses { get; }
    }
}
