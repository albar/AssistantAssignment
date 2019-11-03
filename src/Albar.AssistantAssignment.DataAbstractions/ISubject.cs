using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.DataAbstractions
{
    public interface ISubject : IEquatable<ISubject>
    {
        int Id { get; }
        string Code { get; }
        ImmutableArray<int> Assistants { get; }
        ImmutableArray<int> Schedules { get; }
        int AssistantCountPerScheduleRequirement { get; }
    }
}