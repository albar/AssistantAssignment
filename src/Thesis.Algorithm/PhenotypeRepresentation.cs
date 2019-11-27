using System.Collections.Immutable;
using Thesis.DataType;

namespace Thesis.Algorithm
{
    public class PhenotypeRepresentation
    {
        public ImmutableDictionary<Assesments, double> AssesmentsValues { get; set; }
        public bool IsCollided { get; set; }
        public bool IsAboveThreshold { get; set; }
    }
}
