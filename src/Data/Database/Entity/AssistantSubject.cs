using System.Collections.Generic;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Data.Database.Entity
{
    public class AssistantSubject
    {
        public int AssistantId { get; set; }
        public Assistant Assistant { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public Dictionary<Assesments, double> Assessments { get; set; }
    }
}
