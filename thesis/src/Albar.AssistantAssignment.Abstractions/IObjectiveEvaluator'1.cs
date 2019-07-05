using System;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IObjectiveEvaluator<T> where T : Enum
    {
        double Evaluate(IAssignmentChromosome<T> chromosome);
    }
}