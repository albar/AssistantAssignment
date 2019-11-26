using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Thesis.DataType;

namespace Thesis.Database.Entity
{
    public class Subject
    {
        public int Id { get; set; }
        public Group Group { get; set; }
        public string Code { get; set; }
        public List<Schedule> Schedules { get; set; }
        public List<AssistantSubject> AssistantSubjects { get; set; }
        public int AssistantPerScheduleCount { get; set; }
        public Dictionary<Assesments, double> AssessmentsThreshold { get; set; }
        
        [NotMapped] public List<Assistant> Assistants => AssistantSubjects?.Select(ass => ass.Assistant).ToList();
    }
}