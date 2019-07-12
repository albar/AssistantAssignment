using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.DataAbstractions
{
    public interface IAssistant : IEquatable<IAssistant>
    {
        int Id { get; }
        ImmutableArray<int> Subjects { get; }
    }
}