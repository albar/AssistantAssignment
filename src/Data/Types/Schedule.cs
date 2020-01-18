using System;

namespace AssistantAssignment.Data.Types
{
    public class Schedule
    {
        public Schedule(int courseId,
            int required,
            DayOfWeek day,
            SessionOfDay session)
        {
            CourseId = courseId;
            RequiredAssistantsCount = required;
            Day = day;
            Session = session;
        }

        public int CourseId { get; }
        public int RequiredAssistantsCount { get; }
        public DayOfWeek Day { get; }
        public SessionOfDay Session { get; }

        public bool TimeEquals(Schedule otherSchedule)
        {
            return Day.Equals(otherSchedule.Day) &&
                Session.Equals(otherSchedule.Session);
        }
    }
}
