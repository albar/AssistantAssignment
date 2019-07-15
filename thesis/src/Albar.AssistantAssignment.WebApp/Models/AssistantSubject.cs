using System.Collections.Generic;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.WebApp.Models
{
    public class AssistantSubject
    {
        public int AssistantId { get; set; }
        public Assistant Assistant { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public Dictionary<AssistantAssessment, double> Assessments { get; set; }
    }
}