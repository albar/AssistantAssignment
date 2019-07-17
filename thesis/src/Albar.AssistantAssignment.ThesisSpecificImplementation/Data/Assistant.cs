using System.Collections.Generic;
using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.Data
{
    public class Assistant : IAssistant
    {
        public Assistant(
            int id,
            ImmutableArray<ISubject> subjects,
            ImmutableDictionary<ISubject, Dictionary<AssistantAssessment, double>> subjectAssessments)
        {
            Id = id;
            Subjects = subjects;
            SubjectAssessments = subjectAssessments;
        }

        public int Id { get; set; }
        public string Npm { get; set; }
        public ImmutableArray<ISubject> Subjects { get; set; }
        public ImmutableDictionary<ISubject, Dictionary<AssistantAssessment, double>> SubjectAssessments { get; set; }

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
            return Id.GetHashCode();
        }
    }
}