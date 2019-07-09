using System;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IReproductionSelection<T> : IMutationSelection<T>, ICrossoverSelection<T> where T : Enum
    {
    }
}