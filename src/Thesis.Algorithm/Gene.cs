using System;
using System.Collections.Immutable;
using System.Linq;

namespace Thesis.Algorithm
{
    public struct Gene : IEquatable<Gene>
    {
        private readonly int _hashCode;

        public Gene(ImmutableHashSet<int> assistantsIds)
        {
            AssistantsIds = assistantsIds;
            unchecked
            {
                _hashCode = AssistantsIds.OrderBy(id => id)
                    .Aggregate(1, (accumulated, id) => (accumulated * 397) ^ id);
            }
        }

        public ImmutableHashSet<int> AssistantsIds { get; }

        public override bool Equals(object other)
        {
            return other is Gene gene && Equals(gene);
        }

        public bool Equals(Gene other)
        {
            return AssistantsIds.SetEquals(other.AssistantsIds);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
