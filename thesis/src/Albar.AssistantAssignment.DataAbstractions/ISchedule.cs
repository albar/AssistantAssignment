using System;

namespace Albar.AssistantAssignment.DataAbstractions
{
    public interface ISchedule : IEquatable<ISchedule>
    {
        byte[] Id { get; }
        byte[] Subject { get; }
    }
}