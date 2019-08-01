using System;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.Data
{
    public class Schedule : ISchedule
    {
        public Schedule(
            int id,
            int subject,
            DayOfWeek day,
            SessionOfDay session,
            int lab)
        {
            Id = id;
            Subject = subject;
            Day = day;
            Session = session;
            Lab = lab;
        }

        public int Id { get; set; }
        public int Subject { get; set; }
        public DayOfWeek Day { get; set; }
        public SessionOfDay Session { get; set; }
        public int Lab { get; set; }

        public bool Equals(Schedule other)
        {
            return Day == other.Day && Session == other.Session && Lab == other.Lab;
        }

        public bool Equals(ISchedule other)
        {
            return other != null && other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) ||
                   (obj is Schedule other ? Equals(other) : obj is ISchedule sch && Equals(sch));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Day;
                hashCode = (hashCode * 397) ^ (int) Session;
                hashCode = (hashCode * 397) ^ Lab;
                return hashCode;
            }
        }
    }
}