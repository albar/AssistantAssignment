using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.WebApp.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public Group Group { get; set; }
        public string Code { get; set; }
        public List<Schedule> Schedules { get; set; }
        public List<AssistantSubject> AssistantSubjects { get; set; }
        public int AssistantPerScheduleCount { get; set; }
        public Dictionary<AssistantAssessment, double> AssessmentsThreshold { get; set; }
        
        [NotMapped] public List<Assistant> Assistants => AssistantSubjects?.Select(ass => ass.Assistant).ToList();
    }
}