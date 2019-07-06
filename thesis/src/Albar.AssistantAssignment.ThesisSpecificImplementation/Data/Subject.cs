using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.Data
{
    public class Subject : ISubject
    {
        public Subject(byte[] id, int assistantCountPerScheduleRequirement)
        {
            Id = id;
            AssistantCountPerScheduleRequirement = assistantCountPerScheduleRequirement;
        }

        public byte[] Id { get; }
        public ImmutableArray<byte[]> Assistants { get; set; }
        public ImmutableArray<byte[]> Schedules { get; set; }
        public int AssistantCountPerScheduleRequirement { get; }

        private bool Equals(Subject other)
        {
            return Equals(Id, other.Id);
        }

        public bool Equals(ISubject other)
        {
            return Equals((Subject) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Subject) obj);
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }
    }
}