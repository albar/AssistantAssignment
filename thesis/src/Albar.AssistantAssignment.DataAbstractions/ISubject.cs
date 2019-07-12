using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.DataAbstractions
{
    public interface ISubject : IEquatable<ISubject>
    {
        int Id { get; }
        ImmutableArray<IAssistant> Assistants { get; }
        ImmutableArray<ISchedule> Schedules { get; }
        int AssistantCountPerScheduleRequirement { get; }
    }
}