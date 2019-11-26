using System;
using System.Collections.Immutable;

namespace Thesis.DataType
{
    public class Course
    {
        public Course(int id, ImmutableHashSet<int> assistantsIds)
        {
            Id = id;
            AssistantsIds = assistantsIds;
        }

        public int Id { get; }
        public ImmutableHashSet<int> AssistantsIds { get; }
    }
}
