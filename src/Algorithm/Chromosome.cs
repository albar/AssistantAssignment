using System;
using System.Collections.Immutable;
using System.Linq;
using EvolutionaryAlgorithm.Abstraction;

namespace AssistantAssignment.Algorithm
{
    public class Chromosome : IChromosome<ObjectivesValue>
    {
        private readonly int _hashCode;

        public Chromosome(ImmutableArray<Gene> genotype)
        {
            Genotype = genotype;
            unchecked
            {
                _hashCode = Genotype.Aggregate(1, (accumulated, gene) =>
                    (accumulated * 397) ^ gene.GetHashCode());
            }
        }

        public ObjectivesValue Fitness { get; set; }
        public ObjectivesValue OriginalObjectivesValue { get; set; }
        public ImmutableArray<Gene> Genotype { get; }
        public ImmutableArray<PhenotypeRepresentation> Phenotype { get; set; }

        public override bool Equals(object other)
        {
            return other is Chromosome chromosome && Equals(chromosome);
        }

        public bool Equals(IChromosome<ObjectivesValue> other)
        {
            return other is Chromosome chromosome && Equals(chromosome);
        }

        public bool Equals(Chromosome other)
        {
            return Genotype.SequenceEqual(other.Genotype);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
