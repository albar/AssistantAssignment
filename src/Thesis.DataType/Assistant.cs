using System.Collections.Immutable;

namespace Thesis.DataType
{
    public class Assistant
    {
        public Assistant(int id,
            ImmutableDictionary<int, ImmutableDictionary<Assesments, double>> coursesAssesmentsValues)
        {
            Id = id;
            CoursesAssesmentsValues = coursesAssesmentsValues;
        }

        public int Id { get; }
        public ImmutableDictionary<int, ImmutableDictionary<Assesments, double>> CoursesAssesmentsValues { get; }
    }
}
