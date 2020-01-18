using System;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Data.Database.Entity
{
    public class Schedule
    {
        public int Id { get; set; }
        public Group Group { get; set; }
        public Subject Subject { get; set; }
        public DayOfWeek Day { get; set; }
        public SessionOfDay Session { get; set; }
        public int Lab { get; set; }
    }
}
