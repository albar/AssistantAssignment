using System;
using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IAssistantCombination : IEquatable<IAssistantCombination>
    {
        int Id { get; }
        int Subject { get; }
        ImmutableArray<int> Assistants { get; }
    }
}