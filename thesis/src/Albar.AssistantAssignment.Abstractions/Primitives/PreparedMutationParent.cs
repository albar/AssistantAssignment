using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.Abstractions.Primitives
{
    public struct PreparedMutationParent<T> where T : Enum
    {
        public PreparedMutationParent(ImmutableArray<bool> schema, IAssignmentChromosome<T> parent)
        {
            Schema = schema;
            Parent = parent;
        }

        public ImmutableArray<bool> Schema { get; }
        public IAssignmentChromosome<T> Parent { get; }
    }
}