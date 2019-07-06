using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IAssistantCombination : IEquatable<IAssistantCombination>
    {
        byte[] Id { get; }
        byte[] Subject { get; }
        ImmutableArray<byte[]> Assistants { get; }
    }
}