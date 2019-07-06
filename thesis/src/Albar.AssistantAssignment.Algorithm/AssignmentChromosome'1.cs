using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Albar.AssistantAssignment.Algorithm
{
    public class AssignmentChromosome<T> : IAssignmentChromosome<T> where T : Enum
    {
        public AssignmentChromosome(ImmutableArray<byte> genotype)
        {
            Genotype = genotype;
        }

        public double Fitness { get; set; }
        public ImmutableArray<byte> Genotype { get; }
        public IEnumerable<IScheduleSolutionRepresentation> Phenotype { get; set; }
        ImmutableArray<object> IChromosome.Genotype => Genotype.Cast<object>().ToImmutableArray();
        object IChromosome.Phenotype => Phenotype;
        public ObjectiveValues<T> ObjectiveValues { get; set; }

        public bool Equals(IAssignmentChromosome<T> other)
        {
            return Genotype.Equals(other.Genotype);
        }

        public bool Equals(IChromosome other)
        {
            return other is IAssignmentChromosome<T> chromosome && Equals(chromosome);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IChromosome) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Genotype.Aggregate(1, (hashCode, gene) => (hashCode * 397) ^ gene);
            }
        }
    }
}