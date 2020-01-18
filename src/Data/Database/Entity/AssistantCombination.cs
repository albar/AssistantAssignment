using System.Collections.Generic;

namespace AssistantAssignment.Data.Database.Entity
{
    public class AssistantCombination
    {
        public int Id { get; set; }
        public Subject Subject { get; set; }
        public List<int> Assistants { get; set; }
    }
}
