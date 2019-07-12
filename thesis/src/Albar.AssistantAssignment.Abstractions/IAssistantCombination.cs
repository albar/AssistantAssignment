using System;
using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IAssistantCombination : IEquatable<IAssistantCombination>
    {
        int Id { get; }
        ISubject Subject { get; }
        ImmutableArray<IAssistant> Assistants { get; }
    }
}