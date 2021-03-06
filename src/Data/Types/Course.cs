using System.Collections.Immutable;

namespace AssistantAssignment.Data.Types
{
    public class Course
    {
        public Course(int id, ImmutableHashSet<int> assistantsIds,
            ImmutableDictionary<Assesments, double> threshold)
        {
            Id = id;
            AssistantsIds = assistantsIds;
            Threshold = threshold;
        }

        public int Id { get; }
        public ImmutableHashSet<int> AssistantsIds { get; }
        public ImmutableDictionary<Assesments, double> Threshold { get; }
    }
}
