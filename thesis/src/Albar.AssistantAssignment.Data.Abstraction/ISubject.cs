using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.Data.Abstraction
{
    public interface ISubject : IEquatable<ISubject>
    {
        byte[] Id { get; }
        ImmutableArray<byte[]> Assistants { get; }
        ImmutableArray<byte[]> Schedules { get; }
        int AssistantCountPerScheduleRequirement { get; }
    }
}