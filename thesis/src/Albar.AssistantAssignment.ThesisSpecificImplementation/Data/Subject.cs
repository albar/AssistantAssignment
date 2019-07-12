using System.Collections.Generic;
using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.Data
{
    public class Subject : ISubject
    {
        public Subject(
            int id, int assistantCountPerScheduleRequirement,
            IReadOnlyDictionary<AssistantAssessment, double> assessmentThreshold)
        {
            Id = id;
            AssistantCountPerScheduleRequirement = assistantCountPerScheduleRequirement;
            AssessmentThreshold = assessmentThreshold;
        }

        public int Id { get; }
        public ImmutableArray<int> Assistants { get; set; }
        public ImmutableArray<int> Schedules { get; set; }
        public int AssistantCountPerScheduleRequirement { get; }
        public IReadOnlyDictionary<AssistantAssessment, double> AssessmentThreshold { get; }

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
            return Id.GetHashCode();
        }
    }
}