using System.Collections.Immutable;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Algorithm
{
    public class PhenotypeRepresentation
    {
        public ImmutableDictionary<Assesments, double> AssesmentsValues { get; set; }
        public bool IsCollided { get; set; }
        public bool IsAboveThreshold { get; set; }
    }
}
