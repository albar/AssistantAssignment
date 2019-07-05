using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.Data.Abstraction
{
    public interface IAssistantCombination : IEquatable<IAssistantCombination>
    {
        byte[] Id { get; }
        byte[] Subject { get; }
        ImmutableArray<byte[]> Assistants { get; }
    }
}