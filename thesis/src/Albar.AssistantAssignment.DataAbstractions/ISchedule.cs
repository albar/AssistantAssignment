using System;

namespace Albar.AssistantAssignment.DataAbstractions
{
    public interface ISchedule : IEquatable<ISchedule>
    {
        int Id { get; }
        ISubject Subject { get; }
        DayOfWeek Day { get; }
        SessionOfDay Session { get; }
        int Lab { get; }
    }
}