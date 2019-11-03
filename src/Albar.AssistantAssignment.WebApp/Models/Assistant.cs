using System.Collections.Generic;

namespace Albar.AssistantAssignment.WebApp.Models
{
    public class Assistant
    {
        public int Id { get; set; }
        public Group Group { get; set; }
        public Npm Npm { get; set; }
        public List<AssistantSubject> AssistantSubjects { get; set; }
    }
}