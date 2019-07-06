using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.Data
{
    public sealed class Assistant : IAssistant
    {
        public Assistant(byte[] id, ImmutableArray<byte[]> subjects)
        {
            Id = id;
            Subjects = subjects;
        }

        public byte[] Id { get; }
        public ImmutableArray<byte[]> Subjects { get; }

        private bool Equals(Assistant other)
        {
            return Equals(Id, other.Id);
        }

        public bool Equals(IAssistant other)
        {
            return Equals((Assistant) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Assistant) obj);
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }
    }
}