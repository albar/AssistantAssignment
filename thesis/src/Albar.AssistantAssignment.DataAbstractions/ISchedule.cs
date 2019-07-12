using System;

namespace Albar.AssistantAssignment.DataAbstractions
{
    public interface ISchedule : IEquatable<ISchedule>
    {
        int Id { get; }
        int Subject { get; }
    }
}