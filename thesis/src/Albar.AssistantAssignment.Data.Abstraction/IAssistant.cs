using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.Data.Abstraction
{
    public interface IAssistant : IEquatable<IAssistant>
    {
        byte[] Id { get; }
        ImmutableArray<byte[]> Subjects { get; }
    }
}