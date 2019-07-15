using System.Collections.Generic;

namespace Albar.AssistantAssignment.WebApp.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Subject> Subjects { get; set; }
    }
}