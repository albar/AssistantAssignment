using System;
using System.Collections.Immutable;

namespace Albar.AssistantAssignment.Abstractions.Primitives
{
    public struct PreparedCrossoverParent<T> where T : Enum
    {
        public PreparedCrossoverParent(ImmutableArray<bool> schema, IAssignmentChromosome<T> parent1, IAssignmentChromosome<T> parent2)
        {
            Schema = schema;
            Parent1 = parent1;
            Parent2 = parent2;
        }

        public ImmutableArray<bool> Schema { get; } 
        public IAssignmentChromosome<T> Parent1 { get; }
        public IAssignmentChromosome<T> Parent2 { get; }
    }
}