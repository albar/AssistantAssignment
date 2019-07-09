using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.Algorithm
{
    public class AssistantCombination : IAssistantCombination
    {
        public byte[] Id { get; }
        public byte[] Subject { get; }
        public ImmutableArray<byte[]> Assistants { get; }

        public AssistantCombination(byte[] id, ISubject subject, IEnumerable<IAssistant> assistants)
            : this(id, subject.Id, assistants.Select(a => a.Id))
        {
        }

        public AssistantCombination(byte[] id, byte[] subject, IEnumerable<byte[]> assistants)
        {
            Id = id;
            Subject = subject;
            Assistants = assistants.ToImmutableArray();
        }

        public bool Equals(IAssistantCombination other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Assistants.Length == other.Assistants.Length &&
                   Assistants.All(assistant => other.Assistants.Contains(assistant));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IAssistantCombination) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Assistants.Aggregate(
                    ByteConverter.ToInt32(Subject),
                    (hashCode, assistant) => (hashCode * 397) ^ assistant.GetHashCode()
                );
            }
        }
    }
}