using System;
using System.Threading;
using System.Threading.Tasks;

namespace AssistantAssignment.Algorithm.ObjectiveValueCalculators
{
    public abstract class ObjectiveValueCalculatorBase : IEquatable<ObjectiveValueCalculatorBase>
    {
        public abstract Objectives Objective { get; }
        public abstract bool NeedToBeNormalized { get; }
        public abstract Optimum Optimum { get; }
        public abstract Task<double> CalculateAsync(Chromosome chromosome, CancellationToken token);

        public bool Equals(ObjectiveValueCalculatorBase other)
        {
            return Objective.Equals(other.Objective);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int) Objective;
            }
        }
    }
}
