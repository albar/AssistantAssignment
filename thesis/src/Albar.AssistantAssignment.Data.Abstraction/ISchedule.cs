using System;

namespace Albar.AssistantAssignment.Data.Abstraction
{
    public interface ISchedule : IEquatable<ISchedule>
    {
        byte[] Id { get; }
        byte[] Subject { get; }
    }
}