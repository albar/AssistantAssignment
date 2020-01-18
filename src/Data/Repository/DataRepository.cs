using System.Collections.Immutable;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Data.Repository
{
    public class DataRepository : IDataRepository
    {
        public DataRepository(int id,
            ImmutableArray<Schedule> schedules,
            ImmutableArray<Assistant> assistants,
            ImmutableArray<Course> courses)
        {
            Id = id;
            Schedules = schedules;
            Assistants = assistants;
            Courses = courses;
        }

        public int Id { get; }
        public ImmutableArray<Schedule> Schedules { get; }
        public ImmutableArray<Assistant> Assistants { get; }
        public ImmutableArray<Course> Courses { get; }
    }
}
