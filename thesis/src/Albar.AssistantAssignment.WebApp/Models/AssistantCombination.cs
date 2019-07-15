using System.Collections.Generic;

namespace Albar.AssistantAssignment.WebApp.Models
{
    public class AssistantCombination
    {
        public int Id { get; set; }
        public Subject Subject { get; set; }
        public List<int> Assistants { get; set; }
    }
}