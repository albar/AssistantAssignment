using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.DataAbstractions
{
    public interface IAssistant : IEquatable<IAssistant>
    {
        byte[] Id { get; }
        ImmutableArray<byte[]> Subjects { get; }
    }
}